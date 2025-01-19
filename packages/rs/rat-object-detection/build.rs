use std::{
    io::{BufReader, Read},
    path::{Path, PathBuf},
};

use bytes::Bytes;
use eyre::ContextCompat;
use tokio::io::AsyncWriteExt as _;

fn filter_by_target(target: &str) -> bool {
    if cfg!(target_os = "linux") {
        return target.contains("linux");
    } else if cfg!(target_os = "macos") {
        return target.contains("macos");
    } else if cfg!(target_os = "windows") {
        return target.contains("windows");
    }

    false
}

async fn extract_so_files_to_dir(
    binary: Bytes,
    location: &str,
    extension: &str,
    out_dir: &Path,
) -> eyre::Result<()> {
    let mut tar = tar::Archive::new(flate2::read::GzDecoder::new(
        BufReader::new(binary.as_ref()),
    ));
    let out_dir = Path::new(out_dir).to_path_buf();

    for entry in tar.entries()? {
        let mut entry = entry?;

        let path = entry.path()?;
        if path.extension().unwrap_or_default() == extension
            && path.starts_with(location)
        {
            let file_name = path
                .file_name()
                .wrap_err("Can't get name")?
                .to_str()
                .wrap_err("Can't convert to str")?;
            let file_path = out_dir.join(file_name);
            let mut file = tokio::fs::OpenOptions::new()
                .create(true)
                .truncate(true)
                .write(true)
                .open(file_path)
                .await?;

            let mut buff = Vec::new();

            entry.read_to_end(&mut buff)?;

            file.write_all(&buff).await?;
        }
    }

    Ok(())
}

#[tokio::main]
pub async fn main() -> eyre::Result<()> {
    println!("cargo:rerun-if-changed=build.rs");

    if cfg!(feature = "onnx-acl")
        && cfg!(target_os = "linux")
        && cfg!(target_arch = "aarch64")
    {
        // This is needed to put the build artifacts in the correct location
        let out_dir = std::env::var("CARGO_TARGET_DIR");
        if out_dir.is_err() {
            return Ok(());
        }
        let out_dir = out_dir?;

        // This is needed to put the build artifacts in the correct location
        let target = std::env::var("TARGET");
        if target.is_err() {
            return Ok(());
        }
        let target = target?;

        // This is needed to put the build artifacts in the correct location
        let profile = std::env::var("PROFILE");
        if profile.is_err() {
            return Ok(());
        }
        let profile = profile?;

        let release = octocrab::instance()
            .repos("ARM-software", "ComputeLibrary")
            .releases()
            .get_latest()
            .await?;

        let out_dir = PathBuf::from(out_dir).join(target).join(profile);

        std::fs::create_dir_all(&out_dir)?;

        for asset in release
            .assets
            .iter()
            .filter(|a| a.name.contains("aarch64-cpu-gpu-bin"))
            .filter(|a| a.name.ends_with(".tar.gz"))
            .filter(|a| filter_by_target(&a.name))
        {
            let asset = asset.clone();
            let response =
                reqwest::get(asset.browser_download_url.clone()).await?;

            let bytes = response.bytes().await?;

            // remove extensions from the file name
            let container = asset.name.replace(".tar.gz", "");

            let loc = format!("{}/{}", container, "lib/armv8a-neon-cl");

            extract_so_files_to_dir(bytes, &loc, "so", &out_dir).await?;
        }
    }

    Ok(())
}
