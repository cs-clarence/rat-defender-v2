using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;
using uniffi.rat_object_detection;

namespace RatDefender.Infrastructure.ObjectDetection.Configurations;

public enum VideoCaptureSource
{
    DeviceIndex,
    FilePath,
}

public enum VideoCaptureApis
{
    Gstreamer,
    V4l2,
}


public enum VideoCaptureModes
{
    Bgr,
    Rgb,
    Gray,
    Yuyv,
}

public record RatDetectionImageProcessorOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(RatDetectionImageProcessor).FullNameSection()!}";

    [Required]
    public VideoCaptureSource VideoCaptureSource { get; init; } =
        VideoCaptureSource.DeviceIndex;

    public uint? VideoCaptureIndex { get; init; } = 0;
    public string? VideoCaptureFilePath { get; init; }
    public string? OnnxModelPath { get; init; }
    public bool ShowLabel { get; init; } = true;
    public bool ShowConfidence { get; init; } = true;
    public uint? VideoCaptureWidth { get; init; }
    public uint? VideoCaptureHeight { get; init; }
    public ushort? VideoCaptureFps { get; init; }
    public VideoCaptureApis? VideoCaptureApi { get; init; }
    public VideoCaptureModes? VideoCaptureMode { get; init; }
    public bool DetectRats { get; init; } = true;
}

[OptionsValidator]
public partial class
    RatImageDetectionImageProcessorOptionsValidator : IValidateOptions<
    RatDetectionImageProcessorOptions>;