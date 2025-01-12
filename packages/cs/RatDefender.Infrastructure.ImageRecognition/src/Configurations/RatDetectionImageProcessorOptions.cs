using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.ImageRecognition.Configurations;

public class RatDetectionImageProcessorOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(RatDetectionImageProcessor).FullNameSection()!}";
    
    [Required]
    public string OnnxModelPath { get; init; } = "assets/models/best.onnx";
}

[OptionsValidator]
public partial class
    RatImageDetectionImageProcessorOptionsValidator : IValidateOptions<RatDetectionImageProcessorOptions>;