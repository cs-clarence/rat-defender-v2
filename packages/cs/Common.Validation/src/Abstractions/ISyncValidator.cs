using System.ComponentModel.DataAnnotations;

namespace Common.Validation.Abstractions;

public interface ISyncValidator<in T>
{
    ValidationResult? Validate(T obj);

    bool IsValid(T obj)
    {
        return Validate(obj) == ValidationResult.Success;
    }
}
