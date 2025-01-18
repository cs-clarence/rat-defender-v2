use crate::BoundingBox;

#[derive(Debug, Clone, uniffi::Record)]
pub struct Detection {
    pub label: String,
    pub probability: f32,
    pub bounding_box: BoundingBox,
}
