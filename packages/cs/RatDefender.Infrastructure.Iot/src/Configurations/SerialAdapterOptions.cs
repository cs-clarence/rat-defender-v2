using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record SerialAdapterOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(SerialAdapter).FullNameSection()!}";

    [Required] public string PortName { get; set; } = "/dev/ttyACM0";
    public int? BaudRate { get; set; } = 115200;
}

[OptionsValidator]
public partial class SerialAdapterOptionsValidator : IValidateOptions<
    SerialAdapterOptions>;