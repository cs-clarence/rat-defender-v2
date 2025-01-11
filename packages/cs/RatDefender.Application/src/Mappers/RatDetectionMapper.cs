using System.Security;
using RatDefender.Application.Dtos;
using RatDefender.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace RatDefender.Application.Mappers;

[Mapper]
public static partial class RatDetectionMapper
{
    public static partial RatDetectionDto MapToDto(
        this RatDetection ratDetection);

    public static partial IEnumerable<RatDetectionDto> MapToDto(
        this IEnumerable<RatDetection> ratDetections);
}