use std::{
    ops::Deref,
    sync::{Arc, Mutex},
    thread::available_parallelism,
};

use eyre::{Context, EyreContext, OptionExt as _, eyre};
use ndarray::{Array, Axis, Ix3, s};
use num_traits::Num;
use opencv::{
    core::{self, DataType, Mat, MatTraitConst as _},
    imgproc,
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
        errors::{AnyError, OptionExt, ResultExt, bail},
    },
};

#[derive(Debug, uniffi::Object)]
pub struct RatDetector {
    model: Session,
    video_capture: Arc<Mutex<VideoCapture>>,
}

fn create_default_session_builder(
    options: &Option<SessionOptions>,
) -> Result<SessionBuilder, AnyError> {
    let options = options.unwrap_or_default();
    Ok(Session::builder()
        .wrap_err("Could not create default session builder")?
        .with_optimization_level(
            options
                .optimization_level
                .unwrap_or(SessionGraphOptimizationLevel::Level2)
                .into(),
        )
        .wrap_err("Could not create default session builder")?
        .with_intra_threads(
            options
                .intra_threads
                .map(|val| val as usize)
                .unwrap_or_else(num_cpus::get_physical),
        )
        .wrap_err("Could not create default session builder")?)
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

#[derive(Debug, Clone, Copy, uniffi::Record, Default)]
pub struct SessionOptions {
    pub intra_threads: Option<u8>,
    pub optimization_level: Option<SessionGraphOptimizationLevel>,
}

#[derive(Debug, uniffi::Record)]
pub struct RunResult {
    pub boxes: Vec<Detection>,
    pub annotated_frame: Vec<u8>,
    pub frame: Vec<u8>,
}

#[derive(Debug, uniffi::Record, Default)]
pub struct RunArgs {
    /// The minimum confidence for a detection to be considered a valid detection.
    /// Defaults to 0.5.
    #[uniffi(default = Some(0.5))]
    pub min_confidence: Option<f32>,
}

#[uniffi::export]
impl RatDetector {
    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_from_files(
        model_file: String,
        video_capture_file: String,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_file(model_file)
            .map_err(|e| eyre::eyre!(e))?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::from_file(
                &video_capture_file,
                opencv::videoio::CAP_ANY,
            )
            .wrap_err("Could not open video capture file")?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_from_model_bytes_and_video_capture_file(
        model_bytes: Vec<u8>,
        video_capture_file: String,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_memory(&model_bytes)
            .map_err(|e| eyre::eyre!(e))?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::from_file(
                &video_capture_file,
                opencv::videoio::CAP_ANY,
            )
            .map_err(|e| eyre::eyre!(e))?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_from_model_file_and_video_capture_index(
        model_file: String,
        video_capture_index: u32,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_file(model_file)
            .map_err(|e| eyre::eyre!(e))?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::new(
                video_capture_index as i32,
                opencv::videoio::CAP_ANY,
            )
            .map_err(|e| eyre::eyre!(e))?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_from_model_bytes_and_video_capture_index(
        model_bytes: Vec<u8>,
        video_capture_index: u32,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_memory(&model_bytes)
            .map_err(|e| eyre::eyre!(e))?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::new(
                video_capture_index as i32,
                opencv::videoio::CAP_ANY,
            )
            .map_err(|e| eyre::eyre!(e))?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_default_model_and_video_capture_index(
        video_capture_index: u32,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_memory(
                &Assets::get("models/model.onnx")
                    .ok_or_any_error("Could not find model")?
                    .data,
            )
            .map_err(|e| eyre::eyre!(e))?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::new(
                video_capture_index as i32,
                opencv::videoio::CAP_ANY,
            )
            .map_err(|e| eyre::eyre!(e))?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[inline(always)]
    #[uniffi::constructor(default(options = None))]
    pub fn new_default_model_and_video_capture_file(
        video_capture_file: String,
        options: Option<SessionOptions>,
    ) -> Result<Self, AnyError> {
        let model = create_default_session_builder(&options)?
            .commit_from_memory(
                &Assets::get("models/model.onnx")
                    .ok_or_any_error("Could not find model")?
                    .data,
            )
            .map_err_to_any_error()?;

        let video_capture = Arc::new(Mutex::new(
            VideoCapture::from_file(
                &video_capture_file,
                opencv::videoio::CAP_ANY,
            )
            .map_err_to_any_error()?,
        ));

        Ok(Self {
            model,
            video_capture,
        })
    }

    #[uniffi::method(default(args = None))]
    pub fn run(
        &self,
        args: Option<RunArgs>,
    ) -> Result<Vec<Detection>, AnyError> {
        let mut cam = self.video_capture.lock().map_err_to_any_error()?;

        let video_height = cam
            .get(videoio::CAP_PROP_FRAME_HEIGHT)
            .map_err_to_any_error()? as f32;
        let video_width = cam
            .get(videoio::CAP_PROP_FRAME_WIDTH)
            .map_err_to_any_error()? as f32;

        let mut frame = Mat::default();
        let mut converted = Mat::default();
        let mut resized = Mat::default();
        let mut boxes = Vec::new();

        cam.read(&mut frame).map_err_to_any_error()?;

        imgproc::resize_def(&frame, &mut resized, core::Size {
            height: 640,
            width: 640,
        })
        .map_err_to_any_error()?;

        imgproc::cvt_color_def(
            &resized,
            &mut converted,
            imgproc::COLOR_BGR2RGB,
        )
        .map_err_to_any_error()?;

        if !frame.is_continuous() {
            bail!("Frame is not continuous");
        }

        let mut mat = Mat::new_rows_cols_with_default(
            640,
            640,
            core::CV_32FC3,
            core::Scalar::all(0.0),
        )
        .map_err_to_any_error()?;
        converted
            .convert_to(&mut mat, core::CV_32FC3, 1. / 255., 0.)
            .map_err_to_any_error()?;

        let input = Array::<f32, Ix3>::try_from_cv(&mat)?
            .into_shape_with_order((640, 640, 3, 1))
            .map_err_to_any_error()?;

        let input = input.permuted_axes((3, 2, 0, 1));

        let input = input.view();

        let result = self
            .model
            .run(ort::inputs!["images" => input].map_err_to_any_error()?)
            .map_err_to_any_error()?;

        let output = result["output0"]
            .try_extract_tensor::<f32>()
            .map_err_to_any_error()?;
        let output = output.t();
        let output = output.slice(s![.., .., 0]);

        let args = args.unwrap_or_default();
        let min_confidence = args.min_confidence.unwrap_or(0.5);
        for row in output.axis_iter(Axis(0)) {
            let row = row.iter().copied().collect::<Vec<_>>();
            let (class_id, prob) = row
                .iter()
                // skip bounding box coordinates
                .skip(4)
                .enumerate()
                .map(|(index, value)| (index, *value))
                .reduce(|accum, row| if row.1 > accum.1 { row } else { accum })
                .ok_or_any_error(
                    "Could not find the class with the highest probability",
                )?;
            if prob < min_confidence {
                continue;
            }
            let label = YOLOV11_CLASS_LABELS[class_id];
            let label = format!("{}: {}%", label, prob * 100.);

            let w = row[2] / 640. * video_width;
            let h = row[3] / 640. * video_height;

            let x = (row[0] / 640. * video_width) - (w / 2.);
            let y = (row[1] / 640. * video_height) - (h / 2.);
            let bounding_box =
                BoundingBox::from_xywh(x as u64, y as u64, w as u64, h as u64);
            let detection = Detection::new(label, prob, &bounding_box);
            boxes.push(detection);
        }

        for boxes in boxes.iter() {
            let Detection {
                label,
                probability,
                bounding_box,
            } = boxes;
            let label = format!("{}: {}%", label, probability * 100.);

            let (x, y, w, h) = (
                bounding_box.x() as i32,
                bounding_box.y() as i32,
                bounding_box.width() as i32,
                bounding_box.height() as i32,
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
            .map_err_to_any_error()?;

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
            .map_err_to_any_error()?;
        }

        Ok(boxes)
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
