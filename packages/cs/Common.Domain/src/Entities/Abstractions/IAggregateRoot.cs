namespace Common.Domain.Entities.Abstractions;

public interface IAggregateRoot<out TId> : IEntity<TId>;
