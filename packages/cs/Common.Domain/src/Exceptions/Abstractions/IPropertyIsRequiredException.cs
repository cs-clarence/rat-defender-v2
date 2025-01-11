namespace Common.Domain.Exceptions.Abstractions;

public interface IPropertyIsRequiredException : IDomainException
{
    string Property { get; }
}
