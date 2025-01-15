#[derive(Debug, thiserror::Error, uniffi::Object)]
#[repr(transparent)]
#[error(transparent)]
pub struct AnyError {
    #[from]
    error: eyre::Error,
}

impl AnyError {
    pub fn new(error: eyre::Error) -> Self {
        Self { error }
    }
}

pub trait ResultExt<T, E>
where
    E: core::error::Error,
{
    fn map_err_to_any_error(self) -> Result<T, AnyError>
    where
        Self: Sized;
}

pub trait OptionExt<T> {
    fn ok_or_any_error(self, message: impl Into<String>)
    -> Result<T, AnyError>;
}

impl<T, E> ResultExt<T, E> for Result<T, E>
where
    E: core::error::Error,
{
    fn map_err_to_any_error(self) -> Result<T, AnyError> {
        self.map_err(|e| AnyError::new(eyre::eyre!(e.to_string())))
    }
}

impl<T> OptionExt<T> for Option<T> {
    fn ok_or_any_error(
        self,
        message: impl Into<String>,
    ) -> Result<T, AnyError> {
        self.ok_or(AnyError::new(eyre::eyre!(message.into())))
    }
}

pub macro bail($tt:tt) {
    return Err(AnyError::new(eyre::eyre!($tt)));
}

pub macro any_error($tt:tt) {
    return Err(AnyError::new(eyre::eyre!($tt)));
}
