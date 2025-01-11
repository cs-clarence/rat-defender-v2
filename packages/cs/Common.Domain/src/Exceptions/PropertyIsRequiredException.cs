using Common.Domain.Exceptions.Abstractions;

namespace Common.Domain.Exceptions;

/// <summary>
/// Thrown when a property is null, empty, or whitespace.
/// </summary>
/// <param name="property"></param>
/// <param name="innerException"></param>
public class PropertyIsRequiredException(
    string property,
    string domainCode = nameof(Common),
    System.Exception? innerException = default
)
    : DomainException(
        "Property cannot be blank",
        $"{property} cannot be empty, null, or contain only whitespaces",
        domainCode,
        innerException
    ),
        IPropertyIsRequiredException
{
    public string Property => property;
}
