using Common.Domain.Exceptions.Abstractions;

namespace Common.Domain.Exceptions;

public class EntityNotFoundException(
    string entity,
    string message,
    string domain,
    System.Exception? innerException = null
)
    : DomainException($"{entity} not found", message, domain, innerException),
        IEntityNotFoundException
{
    public string Entity => entity;

    public static EntityNotFoundException ForId(
        string entity,
        string id,
        string domain
    )
    {
        return new EntityNotFoundException(
            entity,
            $"Entity {entity} with id {id} not found",
            domain
        );
    }
}
