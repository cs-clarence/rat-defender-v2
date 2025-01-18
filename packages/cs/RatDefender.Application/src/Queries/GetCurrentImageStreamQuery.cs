using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetCurrentImageStreamQuery : IStreamQuery<ImageDto>
{
    public static readonly GetCurrentImageStreamQuery Instance = new();
}