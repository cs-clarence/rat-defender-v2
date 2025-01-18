using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetCurrentImageQuery : IQuery<ImageDto>
{
    public static readonly GetCurrentImageQuery Instance = new();
}