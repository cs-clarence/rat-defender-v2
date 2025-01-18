#![feature(try_blocks, decl_macro)]
#![allow(dead_code)]

uniffi::setup_scaffolding!();

pub(crate) mod assets;
pub(crate) mod utilities;

mod bounding_box;
mod detection;
mod initialize;
mod rat_detector;

pub use bounding_box::BoundingBox;
pub use detection::Detection;
pub use initialize::initialize;
pub use rat_detector::RatDetector;
