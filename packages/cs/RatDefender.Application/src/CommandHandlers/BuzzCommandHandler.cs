using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class BuzzCommandHandler(IBuzzer buzzer) : ICommandHandler<BuzzCommand>
{
    public async ValueTask<Unit> Handle(BuzzCommand command,
        CancellationToken cancellationToken)
    {
        await buzzer.BuzzAsync(command.Tone, command.Duration, cancellationToken);
        return Unit.Value;
    }
}