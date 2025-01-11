namespace Common.Domain.Entities.Abstractions;

public interface IEntity<out TKey>
{
    public TKey Id { get; }
}
