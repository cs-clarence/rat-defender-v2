#[derive(Debug, Clone, Copy, uniffi::Record)]
pub struct BoundingBox {
    pub x: u64,
    pub y: u64,
    pub width: u64,
    pub height: u64,
}

impl BoundingBox {
    pub fn from_xywh(x: u64, y: u64, w: u64, h: u64) -> Self {
        Self {
            x,
            y,
            width: w,
            height: h,
        }
    }
    pub fn x(&self) -> u64 {
        self.x
    }

    pub fn y(&self) -> u64 {
        self.y
    }

    pub fn width(&self) -> u64 {
        self.width
    }

    pub fn height(&self) -> u64 {
        self.height
    }
}
