use ort::execution_providers::{
    CPUExecutionProvider, CUDAExecutionProvider, RKNPUExecutionProvider,
    ROCmExecutionProvider,
};

use crate::utilities::errors::generic_error::{GenericResult, ResultExt as _};

static mut INITIALIZED: bool = false;

#[uniffi::export]
pub fn initialize() -> GenericResult<()> {
    if unsafe { INITIALIZED } {
        return Ok(());
    }

    let mut providers = vec![CPUExecutionProvider::default().build()];

    if cfg!(feature = "onnx_cuda") {
        providers.push(CUDAExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx_rocm") {
        providers.push(ROCmExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx_rknpu") {
        providers.push(RKNPUExecutionProvider::default().build());
    }

    ort::init()
        .with_execution_providers(providers)
        .commit()
        .map_err_to_generic_error()?;

    unsafe {
        INITIALIZED = true;
    }

    Ok(())
}
