namespace RatDefender.Domain.Services.Abstractions;

public interface IBuzzer
{
    public Task BuzzAsync(ushort tone, ushort duration,
        CancellationToken cancellationToken = default);

    public Task BuzzAsync(CancellationToken cancellationToken = default);
}