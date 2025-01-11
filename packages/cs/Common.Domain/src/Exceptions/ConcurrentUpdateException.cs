using Common.Domain.Exceptions.Abstractions;

namespace Common.Domain.Exceptions;

public class ConcurrentUpdateException(
    string entityType,
    string domainCode = nameof(Common),
    System.Exception? innerException = default
)
    : DomainException(
        "Concurrent updates are not allowed",
        $"An instance of {entityType} is updated concurrently",
        domainCode,
        innerException
    );
