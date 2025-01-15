using Mediator;

namespace RatDefender.Application.Commands;

public record DispenseCommand(ulong Servings = 1u) : ICommand;