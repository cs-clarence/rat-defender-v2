namespace Common.Domain.Entities.Abstractions;

public interface ITimestampedEntity<out TKey> : IEntity<TKey>
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    void Touch();
}