namespace GymBrain.Domain.Common;

/// <summary>
/// A Result monad for returning success/failure without throwing exceptions
/// for expected business failures. This replaces exception-driven flow control.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A success result cannot carry an error.");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("A failure result must carry an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    public static Result<TValue> Failure<TValue>(string error) => new(default, false, error);
}

/// <summary>
/// Generic Result monad carrying a typed value on success.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
