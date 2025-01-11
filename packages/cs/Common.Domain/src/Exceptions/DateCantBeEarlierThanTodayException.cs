namespace Common.Domain.Exceptions;

public class DateCantBeEarlierThanTodayException(
    string domainCode = nameof(Common),
    System.Exception? innerException = null
)
    : TemporalException(
        "Date can't be earlier than today",
        domainCode,
        innerException
    );
