using Mediator;

namespace RatDefender.Application.Commands;

public record SimulateDetectionCommand() : ICommand
{
    public static SimulateDetectionCommand Instance { get; } = new();
}