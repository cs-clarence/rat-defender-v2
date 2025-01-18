using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.ObjectDetection.Configurations;

public enum VideoCaptureSource
{
    DeviceIndex,
    FilePath,
}

public class RatDetectionImageProcessorOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(RatDetectionImageProcessor).FullNameSection()!}";
    
    [Required]
    public VideoCaptureSource VideoCaptureSource { get; init; } = VideoCaptureSource.DeviceIndex;
    public uint? VideoCaptureIndex { get; init; }
    public string? VideoCaptureFilePath { get; init; }
    public string? OnnxModelPath { get; init; }
}

[OptionsValidator]
public partial class
    RatImageDetectionImageProcessorOptionsValidator : IValidateOptions<RatDetectionImageProcessorOptions>;