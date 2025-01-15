ARG ARCH=amd64
ARG BASE_IMAGE=debian:stable-slim
ARG CROSS_TOOLCHAIN_PREFIX=x86_64-linux-gnu-

FROM ${BASE_IMAGE} AS base
ARG ARCH
ARG BASE_IMAGE
ENV ARCH=${ARCH}
ENV BASE_IMAGE=${BASE_IMAGE}
ENV DEBIAN_FRONTEND=noninteractive

RUN mkdir -p /usr/local/src/scripts
COPY ./docker/cross-compile/scripts /usr/local/src/scripts/

RUN ARCH=${ARCH} BASE_IMAGE=${BASE_IMAGE} /usr/local/src/scripts/install-tools.sh
RUN ARCH=${ARCH} BASE_IMAGE=${BASE_IMAGE} /usr/local/src/scripts/install-opencv.sh

FROM base AS base-arm-common

FROM base-arm-common AS base-arm64
ENV CROSS_TOOLCHAIN_PREFIX=aarch64-linux-gnu-
ENV CARGO_TARGET_AARCH64_UNKNOWN_LINUX_GNU_LINKER=${CROSS_TOOLCHAIN_PREFIX}gcc 
ENV AR_aarch64_unknown_linux_gnu=${CROSS_TOOLCHAIN_PREFIX}ar 
ENV CC_aarch64_unknown_linux_gnu=${CROSS_TOOLCHAIN_PREFIX}gcc 
ENV CXX_aarch64_unknown_linux_gnu=${CROSS_TOOLCHAIN_PREFIX}g++ 
ENV RUST_TEST_THREADS=1 
ENV PKG_CONFIG_PATH="/usr/lib/aarch64-linux-gnu/pkgconfig/" 

# Alias for arm64 image
FROM base-arm64 AS base-aarch64

FROM base AS base-x86-common

FROM base-x86-common AS base-amd64

# Alias for amd64 image
FROM base-amd64 AS base-x86_64

FROM base-${ARCH}