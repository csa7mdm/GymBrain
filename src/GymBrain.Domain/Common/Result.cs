namespace GymBrain.Domain.Common;

/// <summary>
/// Represents an error that can be returned from a Result.
/// </summary>
public class Error
{
    public string Message { get; }
    public string Code { get; }

    public Error(string message, string? code = null)
    {
        Message = message;
        Code = code ?? "Error";
    }

    public static Error NullValue(string propertyName) => 
        new Error($"The value for '{propertyName}' cannot be null.", "NullValue");
    
    public static Error NotFound(string entityName, object key) => 
        new Error($"Entity '{entityName}' with key '{key}' was not found.", "NotFound");
    
    public static Error ValidationError(string message) => 
        new Error(message, "Validation");
    
    public static Error Unexpected(string message) => 
        new Error(message, "Unexpected");
    
    public static Error Conflict(string message) => 
        new Error(message, "Conflict");
    
    public static Error Forbidden(string message) => 
        new Error(message, "Forbidden");
    
    public static Error Unauthorized(string message) => 
        new Error(message, "Unauthorized");
}

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A success result cannot carry an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failure result must carry an error.");
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;
    internal Result(TValue? value, bool isSuccess, Error? error) : base(isSuccess, error) => _value = value;
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on a failed result.");
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}