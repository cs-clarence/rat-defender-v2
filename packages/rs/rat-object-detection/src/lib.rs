#![feature(try_blocks, decl_macro)]
#![allow(dead_code)]
#![allow(unused_imports)]

uniffi::setup_scaffolding!();

mod assets;
mod bounding_box;
mod detection;
mod initialize;
mod rat_detector;
mod utilities;

use assets::Assets;
pub use bounding_box::BoundingBox;
use detection::Detection;
pub use initialize::initialize;
use std::sync::Arc;
use std::{path::Path, time::Duration};

use eyre::ContextCompat;
use eyre::OptionExt as _;
use ndarray::{
    Array, Array4, Axis, Dim, Ix2, Ix3, Ix4, Order, ShapeBuilder as _, s,
};
use num_traits::Num;
use opencv::videoio::VideoCaptureTrait;
use opencv::{
    core::{
        self, DataType, LogLevel, Mat, MatTrait, MatTraitConst as _,
        MatTraitConstManual, Point_, Point2d, Point3_,
    },
    highgui, imgproc,
    videoio::{
        self, VideoCapture, VideoCaptureTrait as _, VideoCaptureTraitConst,
    },
};
use ort::environment::Environment;
use ort::execution_providers::CUDAExecutionProvider;
use ort::execution_providers::RKNPUExecutionProvider;
use ort::execution_providers::ROCmExecutionProvider;
use ort::{
    execution_providers::CPUExecutionProvider,
    session::{Session, builder::GraphOptimizationLevel},
};
use rust_embed::Embed;
use tokio::time::Instant;
use tracing::Level;
use utilities::conversions::n_dimentional_arrays::{
    TryFromCv, TryIntoCv, opencv::MatExt as _,
};
use utilities::errors::AnyError;
