using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record DetectionSmsNotifierOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(DetectionSmsNotifier).FullNameSection()!}";

    [Required] public required string ApiCode { get; init; }
    public string? SenderId { get; init; }
    public string? ClientId { get; init; }
    [Required] 
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string MessageFormat { get; init; } =
        "Detected {count} rats at {time}.";

    [Required] public required string[] Recipients { get; init; }
    [Required] public required string Password { get; init; }
}

[OptionsValidator]
public partial class
    DetectionSmsNotifierOptionsValidator : IValidateOptions<
    DetectionSmsNotifierOptions>;