namespace Common.Domain.Exceptions;

public class StartTimeMustBeEarlierThenEndTimeException(
    string domainCode = nameof(Common),
    System.Exception? innerException = null
)
    : TemporalException(
        "Start time must be earlier than end time",
        domainCode,
        innerException
    );
