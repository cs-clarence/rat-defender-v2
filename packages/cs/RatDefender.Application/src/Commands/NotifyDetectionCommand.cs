using Mediator;

namespace RatDefender.Application.Commands;

public record NotifyDetectionCommand(
    ulong DetectionCount = 0
) : ICommand;