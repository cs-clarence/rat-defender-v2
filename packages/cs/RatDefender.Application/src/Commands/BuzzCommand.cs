using Mediator;

namespace RatDefender.Application.Commands;

public record BuzzCommand(
    ushort Tone = 250,
    ushort Duration = 500
) : ICommand;