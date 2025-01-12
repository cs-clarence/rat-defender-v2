using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Commands;

public record RecordDetectionCommand : ICommand<RatDetectionDto>
{
    public static RecordDetectionCommand Instance { get; } = new();
}