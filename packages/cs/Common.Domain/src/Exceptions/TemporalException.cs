using Common.Domain.Exceptions.Abstractions;

namespace Common.Domain.Exceptions;

public class TemporalException(
    string? message,
    string domainCode,
    System.Exception? innerException = null
)
    : DomainException(
        "Temporal error",
        message ?? "Temporal error",
        domainCode,
        innerException
    );
