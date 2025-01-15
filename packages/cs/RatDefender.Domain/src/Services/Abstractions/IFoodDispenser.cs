namespace RatDefender.Domain.Services.Abstractions;

public interface IFoodDispenser
{
    public Task DispenseAsync(ulong servings = 1, CancellationToken cancellationToken = default);
}