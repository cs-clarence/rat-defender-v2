using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record PhoneNumber
{
    [Required]
    public required short CountryCode { get; init; }

    [Required]
    public required long LocalNumber { get; init; }
    
    public string Formatted => $"+{CountryCode}{LocalNumber}";
}

public record DetectionUartSmsNotifierOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(DetectionUartSmsNotifier).FullNameSection()!}";

    [Required] 
    public required string PortName { get; init; } = "/dev/ttyS2";
    
    [Required] 
    public required uint BaudRate { get; init; } = 115200;

    [Required]
    public required string MessageFormat { get; init; } =
        "Detected {{count}} rat(s) at {{time}}.";

    [Required] 
    [ValidateEnumeratedItems]
    public required PhoneNumber[] Recipients { get; init; }
}

[OptionsValidator]
public partial class
    DetectionSmsNotifierOptionsValidator : IValidateOptions<
    DetectionUartSmsNotifierOptions>;