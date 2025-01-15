use crate::BoundingBox;

#[derive(Debug, Clone, uniffi::Record)]
pub struct Detection {
    pub label: String,
    pub probability: f32,
    pub bounding_box: BoundingBox,
}

impl Detection {
    #[inline(always)]
    pub fn new(
        label: String,
        probability: f32,
        bounding_box: &BoundingBox,
    ) -> Self {
        Self {
            label,
            probability,
            bounding_box: *bounding_box,
        }
    }
}
