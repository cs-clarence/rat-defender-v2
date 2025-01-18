#[derive(Debug, Clone, Copy, uniffi::Record)]
pub struct BoundingBox {
    pub x: u64,
    pub y: u64,
    pub width: u64,
    pub height: u64,
}
