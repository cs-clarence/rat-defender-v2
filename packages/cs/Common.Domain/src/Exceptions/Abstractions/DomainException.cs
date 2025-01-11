namespace Common.Domain.Exceptions.Abstractions;

public abstract class DomainException(
    string title,
    string message,
    string domain,
    System.Exception? innerException = null
) : System.Exception(message, innerException), IDomainException
{
    public string Domain => domain;
    public string Title => title;
}
