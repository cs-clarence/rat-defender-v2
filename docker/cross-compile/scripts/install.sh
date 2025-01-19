#!/bin/bash
set -e

echo "Installing tools for ${BASE_IMAGE} ${ARCH}"

if [[ $BASE_IMAGE == debian:* ||  $BASE_IMAGE == ubuntu:* ]]; then
    echo "Running on debian-based image"
    apt update
    apt upgrade -y  

    apt autoremove -y --purge

    if [[ $ARCH == "arm64" || $ARCH == "aarch64" ]]; then
        echo "Installing aarch64 cross compiler"
        # Add the architecture to the sources list
        dpkg --add-architecture arm64
        apt update
        
        # Install aarch64 build tools
        apt install --assume-yes --no-install-recommends  \
            crossbuild-essential-arm64
    fi

    # Build tools for all architectures
    apt install --assume-yes --no-install-recommends  \
        build-essential \
        llvm-dev \
        clang \
        libclang-dev \
        mold \
        pkg-config
    
    # Install OpenCV
    apt install libopencv-dev:${ARCH} -y

    apt autoclean -y 
elif [[ $BASE_IMAGE == fedora:* ]]; then
    echo "Running on fedora-based image"
fi