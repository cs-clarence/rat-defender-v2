namespace WrapperGen.Utilities;

public static class TransformResult
{
    public static TransformResult<TValue, TError> Success<TValue, TError>(
        TValue value)
    {
        return new(value);
    }

    public static TransformResult<TValue, TError> Failure<TValue, TError>(
        TError error)
    {
        return new(error);
    }
}

public record TransformResult<TValue, TError>
{
    private bool _isSuccess;
    public TValue? Value { get; set; } = default;
    public TError? Error { get; set; } = default;

    public TransformResult(TValue value)
    {
        _isSuccess = true;
        Value = value;
    }
    
    public TransformResult(TError error)
    {
        _isSuccess = false;
        Error = error;
    }

    public static TransformResult<TValue, TError> Success(TValue value)
    {
        return new(value);
    }

    public static TransformResult<TValue, TError> Failure(TError error)
    {
        return new(error);
    }

    private bool IsSuccess => _isSuccess;
    private bool IsFailure => !_isSuccess;

    public void Switch(Action<TValue> onSuccess, Action<TError> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value!);
        }
        else if (IsFailure)
        {
            onFailure(Error!);
        }
        else
        {
            throw new Exception("Unknown state");
        }
    }
}