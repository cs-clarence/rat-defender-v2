namespace Common.Domain.Exceptions;

public class EndTimeMustBeLaterThanStartTimeException(
    string domainCode = nameof(Common),
    System.Exception? innerException = null
)
    : TemporalException(
        "End time must be later than start time",
        domainCode,
        innerException
    );
