use std::sync::{Arc, Mutex};

use ndarray::{Array, Axis, Ix3, s};
use num_traits::Num;
use opencv::{
    core::{self, DataType, Mat, MatTraitConst as _, VectorToVec},
    imgcodecs, imgproc,
    videoio::{
        self, VideoCapture, VideoCaptureTrait as _, VideoCaptureTraitConst as _,
    },
};
use ort::session::{
    Session,
    builder::{GraphOptimizationLevel, SessionBuilder},
};

use crate::{
    BoundingBox,
    assets::Assets,
    detection::Detection,
    utilities::{
        conversions::n_dimentional_arrays::TryFromCv as _,
        errors::generic_error::{
            GenericError, GenericResult, OptionExt as _, ResultExt as _, bail,
        },
    },
};

fn create_default_session_builder(
    options: &Option<SessionOptions>,
) -> GenericResult<SessionBuilder> {
    let options = options.unwrap_or_default();
    Session::builder()
        .map_err_to_generic_error()?
        .with_optimization_level(
            options
                .optimization_level
                .unwrap_or(SessionGraphOptimizationLevel::Level2)
                .into(),
        )
        .map_err_to_generic_error()?
        .with_intra_threads(
            options
                .intra_threads
                .map(|val| val as usize)
                .unwrap_or_else(num_cpus::get_physical),
        )
        .map_err_to_generic_error()
}

#[derive(Debug, Clone, Copy, uniffi::Enum)]
#[repr(u8)]
pub enum SessionGraphOptimizationLevel {
    Disable,
    Level1,
    Level2,
    Level3,
}

impl From<u8> for SessionGraphOptimizationLevel {
    fn from(value: u8) -> Self {
        match value {
            0 => Self::Disable,
            1 => Self::Level1,
            2 => Self::Level2,
            3 => Self::Level3,
            _ => panic!("invalid optimization level"),
        }
    }
}

impl From<SessionGraphOptimizationLevel> for GraphOptimizationLevel {
    fn from(value: SessionGraphOptimizationLevel) -> Self {
        match value {
            SessionGraphOptimizationLevel::Disable => Self::Disable,
            SessionGraphOptimizationLevel::Level1 => Self::Level1,
            SessionGraphOptimizationLevel::Level2 => Self::Level2,
            SessionGraphOptimizationLevel::Level3 => Self::Level3,
        }
    }
}

#[derive(Debug, Clone, Copy, uniffi::Enum, strum::FromRepr)]
#[repr(u64)]
pub enum VideoCaptureApi {
    Any = 0,
    // Vfw = 200,
    // V4l = 200,
    V4l2 = 200,
    Firewire = 300,
    // Fireware = 300,
    // Iee1394 = 300,
    // Dc1394 = 300,
    // Cmu1394 = 300,
    Qt = 500,
    Unicap = 600,
    Dshow = 700,
    Pvapi = 800,
    Openni = 900,
    OpenniAsus = 910,
    Android = 1000,
    Xiapi = 1100,
    Avfoundation = 1200,
    Giaganetix = 1300,
    Msmf = 1400,
    Winrt = 1410,
    Intelperc = 1500,
    Openni2 = 1600,
    Openni2Asus = 1610,
    Gphoto2 = 1700,
    Gstreamer = 1800,
    Ffmpeg = 1900,
    Images = 2000,
    Aravis = 2100,
    OpencvMjpeg = 2200,
    IntelMfx = 2300,
    Xine = 2400,
}

#[derive(Debug, Clone, Copy, uniffi::Enum, strum::FromRepr)]
#[repr(u8)]
pub enum VideoCaptureMode {
    Bgr = 0,
    Rgb = 1,
    Gray = 2,
    Yuyv = 3,
}

#[derive(Debug, Clone, Copy, uniffi::Record, Default)]
pub struct SessionOptions {
    pub intra_threads: Option<u8>,
    pub optimization_level: Option<SessionGraphOptimizationLevel>,
}

#[derive(Debug, Clone, Copy, uniffi::Record, Default)]
pub struct CaptureOptions {
    pub frame_height: Option<u32>,
    pub frame_width: Option<u32>,
    pub fps: Option<u32>,
    pub backend: Option<VideoCaptureApi>,
    pub mode: Option<VideoCaptureMode>,
}

#[derive(Debug, Clone, Copy, uniffi::Record, Default)]
pub struct CtorOptions {
    pub session: Option<SessionOptions>,
    pub capture: Option<CaptureOptions>,
}

fn apply_capture_options(
    options: &Option<CaptureOptions>,
    cam: &mut VideoCapture,
) -> GenericResult<()> {
    let options = options.unwrap_or_default();

    let mode = options.mode;

    if let Some(mode) = mode {
        cam.set(videoio::CAP_PROP_MODE, mode as u8 as f64)
            .map_err_to_generic_error()?;
    }

    let frame_height = options.frame_height;
    if let Some(frame_height) = frame_height {
        cam.set(videoio::CAP_PROP_FRAME_HEIGHT, frame_height as f64)
            .map_err_to_generic_error()?;
    }
    let frame_width = options.frame_width;
    if let Some(frame_width) = frame_width {
        cam.set(videoio::CAP_PROP_FRAME_WIDTH, frame_width as f64)
            .map_err_to_generic_error()?;
    }
    let fps = options.fps;
    if let Some(fps) = fps {
        cam.set(videoio::CAP_PROP_FPS, fps as f64)
            .map_err_to_generic_error()?;
    }

    Ok(())
}

#[uniffi::export]
pub fn new_rat_detector_from_files(
    model_file: String,
    video_capture_file: String,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_file(model_file)
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;

    let video_capture = Arc::new(Mutex::new(
        VideoCapture::from_file(&video_capture_file, cap_api)
            .map_err_to_generic_error()?,
    ));

    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[uniffi::export]
pub fn new_rat_detector_from_model_bytes_and_video_capture_file(
    model_bytes: Vec<u8>,
    video_capture_file: String,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_memory(&model_bytes)
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;
    let video_capture = Arc::new(Mutex::new(
        VideoCapture::from_file(&video_capture_file, cap_api)
            .map_err_to_generic_error()?,
    ));
    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[uniffi::export]
pub fn new_rat_detector_from_model_file_and_video_capture_index(
    model_file: String,
    video_capture_index: u32,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_file(model_file)
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;

    let video_capture = Arc::new(Mutex::new(
        VideoCapture::new(video_capture_index as i32, cap_api)
            .map_err_to_generic_error()?,
    ));
    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[uniffi::export]
pub fn new_rat_detector_from_model_bytes_and_video_capture_index(
    model_bytes: Vec<u8>,
    video_capture_index: u32,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_memory(&model_bytes)
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;
    let video_capture = Arc::new(Mutex::new(
        VideoCapture::new(video_capture_index as i32, cap_api)
            .map_err_to_generic_error()?,
    ));
    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[uniffi::export]
pub fn new_rat_detector_from_default_model_and_video_capture_index(
    video_capture_index: u32,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_memory(
            &Assets::get("models/model.onnx")
                .ok_or_generic_error("Could not find model")
                .expect("Can't load default model")
                .data,
        )
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;
    let video_capture = Arc::new(Mutex::new(
        VideoCapture::new(video_capture_index as i32, cap_api)
            .map_err_to_generic_error()?,
    ));
    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[uniffi::export]
pub fn new_rat_detector_from_default_model_and_video_capture_file(
    video_capture_file: String,
    options: Option<CtorOptions>,
) -> GenericResult<Arc<RatDetector>> {
    let options = options.unwrap_or_default();
    let model = create_default_session_builder(&options.session)?
        .commit_from_memory(
            &Assets::get("models/model.onnx")
                .ok_or_generic_error("Could not find model")
                .expect("Can't load default model")
                .data,
        )
        .map_err_to_generic_error()?;

    let cap_api = options
        .capture
        .unwrap_or_default()
        .backend
        .unwrap_or(VideoCaptureApi::Any) as i32;
    let video_capture = Arc::new(Mutex::new(
        VideoCapture::from_file(&video_capture_file, cap_api)
            .map_err_to_generic_error()?,
    ));
    apply_capture_options(
        &options.capture,
        &mut *video_capture.lock().map_err_to_generic_error()?,
    )?;

    Ok(Arc::new(RatDetector {
        model,
        video_capture,
    }))
}

#[derive(Debug, uniffi::Object)]
pub struct RatDetector {
    pub(crate) model: Session,
    pub(crate) video_capture: Arc<Mutex<VideoCapture>>,
}

#[derive(Debug, Clone, Copy, uniffi::Enum)]
pub enum FrameFormat {
    Jpeg,
    Png,
}

#[derive(Debug, uniffi::Record)]
pub struct RunResult {
    pub detections: Vec<Detection>,
    pub frame: Vec<u8>,
    pub frame_format: FrameFormat,
}

#[derive(Debug, uniffi::Record, Default)]
pub struct RunArgs {
    /// The minimum confidence for a detection to be considered a valid detection.
    pub min_confidence: Option<f32>,
    /// Whether to show labels on the detections.
    pub show_labels: Option<bool>,
    /// Whether to show confidence on the detections.
    pub show_confidence: Option<bool>,
    pub detect_rats: Option<bool>,
}

#[uniffi::export]
impl RatDetector {
    pub fn set_capture_frame_size(
        &self,
        frame_height: u32,
        frame_width: u32,
    ) -> GenericResult<()> {
        self.set_capture_frame_height(frame_height)?;
        self.set_capture_frame_width(frame_width)?;

        Ok(())
    }

    pub fn set_capture_frame_height(
        &self,
        frame_height: u32,
    ) -> GenericResult<()> {
        let mut cam = self.video_capture.lock().map_err_to_generic_error()?;
        cam.set(videoio::CAP_PROP_FRAME_HEIGHT, frame_height as f64)
            .map_err_to_generic_error()?;

        Ok(())
    }

    pub fn get_capture_frame_height(&self) -> GenericResult<u32> {
        let cam = self.video_capture.lock().map_err_to_generic_error()?;
        Ok(cam
            .get(videoio::CAP_PROP_FRAME_HEIGHT)
            .map_err_to_generic_error()? as u32)
    }

    pub fn set_capture_frame_width(
        &self,
        frame_width: u32,
    ) -> GenericResult<()> {
        let mut cam = self.video_capture.lock().map_err_to_generic_error()?;
        cam.set(videoio::CAP_PROP_FRAME_WIDTH, frame_width as f64)
            .map_err_to_generic_error()?;

        Ok(())
    }

    pub fn get_capture_frame_width(&self) -> GenericResult<u32> {
        let cam = self.video_capture.lock().map_err_to_generic_error()?;
        Ok(cam
            .get(videoio::CAP_PROP_FRAME_WIDTH)
            .map_err_to_generic_error()? as u32)
    }

    pub fn get_capture_backend(&self) -> GenericResult<VideoCaptureApi> {
        let cam = self.video_capture.lock().map_err_to_generic_error()?;
        let backend = cam
            .get(videoio::CAP_PROP_BACKEND)
            .map_err_to_generic_error()?;
        VideoCaptureApi::from_repr(backend as u64)
            .ok_or_generic_error("Could not parse backend")
    }

    pub fn set_capture_fps(&self, fps: u32) -> GenericResult<()> {
        let mut cam = self.video_capture.lock().map_err_to_generic_error()?;
        cam.set(videoio::CAP_PROP_FPS, fps as f64)
            .map_err_to_generic_error()?;

        Ok(())
    }

    pub fn get_capture_fps(&self) -> GenericResult<u32> {
        let cam = self.video_capture.lock().map_err_to_generic_error()?;
        Ok(cam.get(videoio::CAP_PROP_FPS).map_err_to_generic_error()? as u32)
    }

    pub fn set_capture_mode(
        &self,
        mode: VideoCaptureMode,
    ) -> GenericResult<()> {
        let mut cam = self.video_capture.lock().map_err_to_generic_error()?;
        cam.set(videoio::CAP_PROP_MODE, mode as u8 as f64)
            .map_err_to_generic_error()?;
        Ok(())
    }

    pub fn get_capture_mode(&self) -> GenericResult<VideoCaptureMode> {
        let cam = self.video_capture.lock().map_err_to_generic_error()?;
        let mode =
            cam.get(videoio::CAP_PROP_MODE).map_err_to_generic_error()?;
        VideoCaptureMode::from_repr(mode as u8)
            .ok_or_generic_error("Could not parse mode")
    }

    pub fn run(&self, args: Option<RunArgs>) -> GenericResult<RunResult> {
        let mut cam = self.video_capture.lock().map_err_to_generic_error()?;
        let args = args.unwrap_or_default();

        let video_height = cam
            .get(videoio::CAP_PROP_FRAME_HEIGHT)
            .map_err_to_generic_error()? as f32;
        let video_width = cam
            .get(videoio::CAP_PROP_FRAME_WIDTH)
            .map_err_to_generic_error()? as f32;

        let mut frame = Mat::default();

        cam.read(&mut frame).map_err_to_generic_error()?;

        let detect_rats = args.detect_rats.unwrap_or(true);

        let mut boxes = Vec::new();

        if detect_rats {
            let mut converted = Mat::default();
            let mut resized = Mat::default();
            imgproc::resize_def(&frame, &mut resized, core::Size {
                height: 640,
                width: 640,
            })
            .map_err_to_generic_error()?;

            imgproc::cvt_color_def(
                &resized,
                &mut converted,
                imgproc::COLOR_BGR2RGB,
            )
            .map_err_to_generic_error()?;

            if !frame.is_continuous() {
                bail!("Frame is not continuous");
            }

            let mut mat = Mat::new_rows_cols_with_default(
                640,
                640,
                core::CV_32FC3,
                core::Scalar::all(0.0),
            )
            .map_err_to_generic_error()?;
            converted
                .convert_to(&mut mat, core::CV_32FC3, 1. / 255., 0.)
                .map_err_to_generic_error()?;

            let input = Array::<f32, Ix3>::try_from_cv(&mat)
                .map_err(|e| GenericError::new(e.to_string()))?
                .into_shape_with_order((640, 640, 3, 1))
                .map_err_to_generic_error()?;

            let input = input.permuted_axes((3, 2, 0, 1));

            let input = input.view();

            let result = self
                .model
                .run(
                    ort::inputs!["images" => input]
                        .map_err_to_generic_error()?,
                )
                .map_err_to_generic_error()?;

            let output = result["output0"]
                .try_extract_tensor::<f32>()
                .map_err_to_generic_error()?;
            let output = output.t();
            let output = output.slice(s![.., .., 0]);

            let min_confidence = args.min_confidence.unwrap_or(0.5);
            for row in output.axis_iter(Axis(0)) {
                let row = row.iter().copied().collect::<Vec<_>>();
                let (class_id, prob) = row
                    .iter()
                    // skip bounding box coordinates
                    .skip(4)
                    .enumerate()
                    .map(|(index, value)| (index, *value))
                    .reduce(
                        |accum, row| if row.1 > accum.1 { row } else { accum },
                    )
                    .ok_or_generic_error(
                        "Could not find the class with the highest probability",
                    )?;
                if prob < min_confidence {
                    continue;
                }
                let label = YOLOV11_CLASS_LABELS[class_id];
                let label = label.to_string();

                let w = row[2] / 640. * video_width;
                let h = row[3] / 640. * video_height;

                let x = (row[0] / 640. * video_width) - (w / 2.);
                let y = (row[1] / 640. * video_height) - (h / 2.);
                let bounding_box = BoundingBox {
                    height: h as u64,
                    width: w as u64,
                    x: x as u64,
                    y: y as u64,
                };
                let detection = Detection {
                    bounding_box,
                    label: label.to_string(),
                    probability: prob,
                };
                boxes.push(detection);
            }

            for boxes in boxes.iter() {
                let Detection {
                    label,
                    probability,
                    bounding_box,
                } = boxes;

                let (x, y, w, h) = (
                    bounding_box.x as i32,
                    bounding_box.y as i32,
                    bounding_box.width as i32,
                    bounding_box.height as i32,
                );

                let rect = core::Rect {
                    height: h,
                    width: w,
                    x,
                    y,
                };

                imgproc::rectangle(
                    &mut frame,
                    rect,
                    core::Scalar::new(255., 0., 0., 255.),
                    2,
                    imgproc::LINE_AA,
                    0,
                )
                .map_err_to_generic_error()?;

                let show_labels = args.show_labels.unwrap_or(true);

                if show_labels {
                    let show_confidence = args.show_confidence.unwrap_or(true);

                    let label = if show_confidence {
                        format!("{}: {}%", label, probability * 100.)
                    } else {
                        label.to_string()
                    };
                    imgproc::put_text(
                        &mut frame,
                        &label,
                        core::Point { x, y },
                        imgproc::FONT_HERSHEY_PLAIN,
                        1.,
                        core::Scalar::new(255., 0., 0., 255.),
                        1,
                        imgproc::LINE_AA,
                        false,
                    )
                    .map_err_to_generic_error()?;
                }
            }
        }

        let mut buf = core::Vector::<u8>::new();
        imgcodecs::imencode_def(".jpg", &frame, &mut buf)
            .map_err_to_generic_error()?;

        Ok(RunResult {
            frame: buf.to_vec(),
            detections: boxes,
            frame_format: FrameFormat::Jpeg,
        })
    }
}

const YOLOV11_CLASS_LABELS: [&str; 1] = ["rat"];

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
struct RgbPixel<T: Num + Copy> {
    r: T,
    g: T,
    b: T,
}

unsafe impl DataType for RgbPixel<f32> {
    fn opencv_depth() -> i32 {
        opencv::core::CV_32F
    }

    fn opencv_channels() -> i32 {
        3
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
struct BgrPixel<T: Num + Copy> {
    b: T,
    g: T,
    r: T,
}

unsafe impl DataType for BgrPixel<f32> {
    fn opencv_depth() -> i32 {
        opencv::core::CV_32F
    }

    fn opencv_channels() -> i32 {
        3
    }
}
