namespace Common.Domain.Exceptions.Abstractions;

public interface IDomainException
{
    string Domain { get; }
    string Title { get; }
    string Message { get; }
}
