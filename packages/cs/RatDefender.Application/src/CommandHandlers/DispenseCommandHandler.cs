using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class DispenseCommandHandler(IFoodDispenser dispenser)
    : ICommandHandler<DispenseCommand>
{
    public async ValueTask<Unit> Handle(DispenseCommand command,
        CancellationToken cancellationToken)
    {
        await dispenser.DispenseAsync(command.Servings, cancellationToken);
        return Unit.Value;
    }
}