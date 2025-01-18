use super::StdResult;

#[derive(thiserror::Error, uniffi::Error, Debug)]
pub enum GenericError {
    #[error("{message}")]
    Generic { message: String },
}

impl GenericError {
    pub fn new(message: impl Into<String>) -> GenericError {
        GenericError::Generic {
            message: message.into(),
        }
    }
}

impl From<String> for GenericError {
    fn from(value: String) -> Self {
        GenericError::Generic { message: value }
    }
}

impl From<&str> for GenericError {
    fn from(value: &str) -> Self {
        GenericError::Generic {
            message: value.to_owned(),
        }
    }
}

pub trait ResultExt<T, E>
where
    E: core::error::Error,
{
    fn map_err_to_generic_error(self) -> StdResult<T, GenericError>
    where
        Self: Sized;
}

pub trait OptionExt<T> {
    fn ok_or_generic_error(
        self,
        message: impl Into<String>,
    ) -> StdResult<T, GenericError>;
}

impl<T, E> ResultExt<T, E> for StdResult<T, E>
where
    E: core::error::Error,
{
    fn map_err_to_generic_error(self) -> StdResult<T, GenericError> {
        self.map_err(|e| GenericError::new(e.to_string()))
    }
}

impl<T> OptionExt<T> for Option<T> {
    fn ok_or_generic_error(
        self,
        message: impl Into<String>,
    ) -> StdResult<T, GenericError> {
        self.ok_or(GenericError::new(message))
    }
}

pub type GenericResult<T> = std::result::Result<T, GenericError>;

pub macro bail($tt:tt) {
    Err(GenericError::new(format!($tt)))?;
}

pub macro generic_error($tt:tt) {
    Err(GenericError::new(format!($tt)));
}
