#!/bin/bash
set -e

echo "Installing OpenCV for ${BASE_IMAGE} ${ARCH}"

if [[ $BASE_IMAGE == debian:* ||  $BASE_IMAGE == ubuntu:* ]]; then
    echo "Running on debian-based image"
    
    apt install libopencv-dev:${ARCH} -y
elif [[ $BASE_IMAGE == fedora:* ]]; then
    echo "Running on fedora-based image"
fi