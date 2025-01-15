using System.ComponentModel.DataAnnotations;
using Common.Configuration.Extensions;
using Microsoft.Extensions.Options;

namespace RatDefender.Infrastructure.Iot.Configurations;

public record ThermalImagerOptions
{
    public static readonly string DefaultKey =
        $"Slices:{typeof(ThermalImager).FullNameSection()!}";

    [Range(0, 7)] public int BusId { get; init; } = 2;
    public int? DeviceAddress { get; init; }
}

[OptionsValidator]
public partial class
    ThermalImagerOptionsValidator : IValidateOptions<ThermalImagerOptions>;