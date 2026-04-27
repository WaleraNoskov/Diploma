namespace Contracts;

/// <summary>
/// Discriminated union: either a value (success) or an <see cref="Error"/> (failure).
/// Handlers never throw; callers pattern-match on IsSuccess.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Fail(Error error) => new(error);
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public Error Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);

    /// <summary>Transforms the value if successful; propagates failure unchanged.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? mapper(_value!) : _error;

    /// <summary>Chains async operations; short-circuits on failure.</summary>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> next) =>
        IsSuccess ? await next(_value!) : _error;

    public override string ToString() =>
        IsSuccess ? $"Ok({_value})" : $"Fail({_error})";
}

/// <summary>Non-generic result for commands that return no value.</summary>
public sealed class Result
{
    public static readonly Result Success = new(true, Error.None);

    private readonly Error _error;

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result Fail(Error error) => new(false, error);
    public static implicit operator Result(Error error) => Fail(error);

    public override string ToString() =>
        IsSuccess ? "Ok" : $"Fail({_error})";
}