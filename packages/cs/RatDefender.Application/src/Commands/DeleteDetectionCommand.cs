using Mediator;

namespace RatDefender.Application.Commands;

public record DeleteDetectionCommand(string Id) : ICommand;