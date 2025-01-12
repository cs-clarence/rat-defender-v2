namespace RatDefender.Domain.Services.Abstractions;

public interface IFoodDispenser
{
    public Task DispenseAsync(ulong count, CancellationToken cancellationToken = default);
}