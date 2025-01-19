use std::path::Path;

use ort::execution_providers::{
    ACLExecutionProvider, ArmNNExecutionProvider, CPUExecutionProvider,
    CUDAExecutionProvider, RKNPUExecutionProvider, ROCmExecutionProvider,
};

use crate::utilities::errors::generic_error::{GenericResult, ResultExt as _};

static mut INITIALIZED: bool = false;

const LIB_FILENAMES: [&str; 4] = [
    "libonnxruntime.so",
    "libonnxruntime",
    "onnxruntime.so",
    "onnxruntime",
];

fn find_onnxruntime_lib() -> Option<String> {
    let exe_path = std::env::current_exe().ok()?.parent()?.to_path_buf();
    let mut parent = exe_path.parent()?.to_path_buf();

    while let Some(cur) = parent.parent() {
        for lib_filename in LIB_FILENAMES.iter() {
            let lib_path = cur.join(lib_filename);
            if lib_path.exists() {
                return Some(lib_path.to_string_lossy().to_string());
            }
        }
        parent = cur.to_path_buf();
    }

    let ld_library_path = std::env::var("LD_LIBRARY_PATH").ok()?;
    for lib_filename in LIB_FILENAMES.iter() {
        for lib_path in ld_library_path.split(':') {
            let lib_path = lib_path.trim();
            let lib_path = Path::new(lib_path);
            let lib_path = lib_path.join(lib_filename);
            if lib_path.exists() {
                return Some(lib_path.to_string_lossy().to_string());
            }
        }
    }

    None
}

#[cfg(feature = "onnx-lib-load-dynamic")]
fn ort_init() -> ort::environment::EnvironmentBuilder {
    let dylib_path = std::env::var("ORT_DYLIB_PATH")
        .map_or_else(|_| find_onnxruntime_lib(), Some)
        .expect("Could not find dynamic library path");

    ort::init_from(dylib_path)
}

#[cfg(not(feature = "onnx-lib-load-dynamic"))]
fn ort_init() -> ort::environment::EnvironmentBuilder {
    ort::init()
}

#[uniffi::export]
pub fn initialize() -> GenericResult<()> {
    if unsafe { INITIALIZED } {
        return Ok(());
    }

    let mut providers = vec![CPUExecutionProvider::default().build()];

    if cfg!(feature = "onnx-cuda") {
        providers.push(CUDAExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx-rocm") {
        providers.push(ROCmExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx-rknpu") {
        providers.push(RKNPUExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx-armnn") {
        providers.push(ArmNNExecutionProvider::default().build());
    }

    if cfg!(feature = "onnx-acl") {
        providers.push(ACLExecutionProvider::default().build());
    }

    ort_init()
        .with_execution_providers(providers)
        .commit()
        .map_err_to_generic_error()?;

    unsafe {
        INITIALIZED = true;
    }

    Ok(())
}
