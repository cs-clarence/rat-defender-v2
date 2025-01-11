namespace Common.Domain.Exceptions.Abstractions;

public interface IEntityNotFoundException : IDomainException
{
    string Entity { get; }
}
