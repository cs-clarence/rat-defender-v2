using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.ObjectDetection.Configurations;

public enum VideoCaptureSource
{
    DeviceIndex,
    FilePath,
}

public record RatDetectionImageProcessorOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(RatDetectionImageProcessor).FullNameSection()!}";
    
    [Required]
    public VideoCaptureSource VideoCaptureSource { get; init; } = VideoCaptureSource.DeviceIndex;

    public uint? VideoCaptureIndex { get; init; } = 0;
    public string? VideoCaptureFilePath { get; init; }
    public string? OnnxModelPath { get; init; }
    public bool ShowLabel { get; init; } = true;
    public bool ShowConfidence { get; init; } = true;
    public uint VideoCaptureWidth { get; init; } = 640;
    public uint VideoCaptureHeight { get; init; } = 480;
    public ushort VideoCaptureFps { get; init; } = 30;
    public bool DetectRats { get; init; } = true;
}

[OptionsValidator]
public partial class
    RatImageDetectionImageProcessorOptionsValidator : IValidateOptions<RatDetectionImageProcessorOptions>;