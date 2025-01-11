using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Common.S3.Configurations;

public record S3FileOperationUrlProviderOptions
{
    public static readonly string DefaultKey = "S3";

    public string? Region { get; set; }

    [Url, Required]
    public required string Endpoint { get; set; }

    [Required]
    public required string AccessKey { get; set; }

    [Required]
    public required string SecretKey { get; set; }
    public bool UseHttpsForGeneratedUrls { get; set; }
    public bool ForcePathStyle { get; set; } = true;
    public bool CreateBucketIfNotExists { get; set; } = false;
}

[OptionsValidator]
public partial class S3FileOperationUrlProviderOptionsValidator
    : IValidateOptions<S3FileOperationUrlProviderOptions>;
