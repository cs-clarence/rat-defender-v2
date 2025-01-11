using System.ComponentModel.DataAnnotations;

namespace Common.Validation.Abstractions;

public interface IValidator<in T> : ISyncValidator<T>, IAsyncValidator<T>
{
    Task<ValidationResult?> IAsyncValidator<T>.ValidateAsync(T obj)
    {
        return Task.FromResult(Validate(obj));
    }
}
