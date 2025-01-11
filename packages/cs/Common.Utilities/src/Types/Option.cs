using System.Diagnostics.CodeAnalysis;

namespace Common.Utilities.Types;

public static class Option
{
    public static Option<T> None<T>() => new Option<T>.None<T>();

    public static Option<T> Some<T>(T value) => new Option<T>.Some<T>(value);
}

public readonly struct Option<T>
{
    public enum UnionKind
    {
        None = 0,
        Some = 1,
    };

    public record struct None<TValue>();

    public record struct Some<TValue>(TValue Value);

    public static implicit operator Option<T>(None<T> value)
    {
        return new Option<T>(UnionKind.None, default!);
    }

    public static implicit operator Option<T>(Some<T> value)
    {
        return new Option<T>(UnionKind.Some, value.Value);
    }

    public static implicit operator Option<T>(T value)
    {
        return new Option<T>(UnionKind.Some, value);
    }

    public static explicit operator None<T>(Option<T> union)
    {
        if (union._kind != UnionKind.None)
        {
            throw new InvalidCastException();
        }

        return new();
    }

    public static explicit operator Some<T>(Option<T> union)
    {
        if (union._kind != UnionKind.Some)
        {
            throw new InvalidCastException();
        }

        return new(union._value!);
    }

    public bool TryGetNone(out None<T> value)
    {
        if (_kind == UnionKind.None)
        {
            value = new();
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetSome(out Some<T> value)
    {
        if (_kind == UnionKind.Some)
        {
            value = new(_value!);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        if (_kind == UnionKind.Some)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    public void MatchSome(Action<T> action)
    {
        if (_kind == UnionKind.Some)
        {
            action(_value!);
        }
    }

    public Task MatchSomeAsync(Func<T, Task> action)
    {
        if (_kind == UnionKind.Some)
        {
            return action(_value!);
        }

        return Task.CompletedTask;
    }

    public void MatchNone(Action action)
    {
        if (_kind == UnionKind.None)
        {
            action();
        }
    }

    public Task MatchNoneAsync(Func<Task> action)
    {
        if (_kind == UnionKind.None)
        {
            return action();
        }

        return Task.CompletedTask;
    }

    public void Match(Action<T> someAction, Action noneAction)
    {
        if (_kind == UnionKind.Some)
        {
            someAction(_value!);
        }
        else
        {
            noneAction();
        }
    }

    public Task MatchAsync(Func<T, Task> someAction, Func<Task> noneAction)
        => _kind == UnionKind.Some ? someAction(_value!) : noneAction();

    public UnionKind Kind => _kind;

    private readonly UnionKind _kind;
    private readonly T _value;

    private Option(UnionKind kind, T value)
    {
        _kind = kind;
        _value = value;
    }

    public override string ToString()
    {
        return _kind switch
        {
            UnionKind.None => "None",
            UnionKind.Some => $"Some({_value})",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public bool IsNone => Kind == UnionKind.None;
    public bool IsSome => Kind == UnionKind.Some;
}