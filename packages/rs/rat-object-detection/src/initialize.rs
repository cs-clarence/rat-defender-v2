use ort::execution_providers::{
    CPUExecutionProvider, CUDAExecutionProvider, RKNPUExecutionProvider,
    ROCmExecutionProvider,
};

use crate::utilities::errors::AnyError;

#[uniffi::export]
pub fn initialize() -> Result<(), AnyError> {
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
        .map_err(|e| eyre::eyre!(e))?;

    Ok(())
}
