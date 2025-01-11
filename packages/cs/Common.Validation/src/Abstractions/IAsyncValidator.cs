using System.ComponentModel.DataAnnotations;

namespace Common.Validation.Abstractions;

public interface IAsyncValidator<in T>
{
    Task<ValidationResult?> ValidateAsync(T obj);

    async Task<bool> IsValidAsync(T obj)
    {
        return await ValidateAsync(obj) == ValidationResult.Success;
    }
}
