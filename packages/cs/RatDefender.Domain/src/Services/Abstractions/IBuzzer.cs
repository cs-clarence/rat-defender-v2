namespace RatDefender.Domain.Services.Abstractions;

public interface IBuzzer
{
    public Task BuzzAsync(uint tone, uint duration, uint delay,
        CancellationToken cancellationToken = default);

    public Task BuzzAsync(uint tone, uint duration,
        CancellationToken cancellationToken = default);

    public Task BuzzAsync(CancellationToken cancellationToken = default);
}