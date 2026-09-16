namespace GymBrain.Domain.Common;

/// <summary>
/// Represents an error that can be returned from a Result.
/// </summary>
public class Error
{
    public string Message { get; }
    public string Code { get; }

    public Error(string message, string code = null)
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

/// <summary>
/// A Result monad for returning success/failure without throwing exceptions
/// for expected business failures. This replaces exception-driven flow control.
/// </summary>
public abstract class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("A success result cannot carry an error.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("A failure result must carry an error.");
        
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new SuccessResult();
    public static Result Failure(Error error) => new FailureResult(error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new SuccessResult<TValue>(value);
    public static Result<TValue> Failure<TValue>(Error error) => new FailureResult<TValue>(error);
}

public class SuccessResult : Result
{
    public SuccessResult() : base(true, null) { }
}

public class SuccessResult<TValue> : Result
{
    public SuccessResult(TValue value) : base(true, null)
    {
        Value = value;
    }

    public TValue Value { get; }
}

public class FailureResult : Result
{
    public FailureResult(Error error) : base(false, error) { }
}

public class FailureResult<TValue> : Result
{
    public FailureResult(TValue value, Error error) : base(false, error)
    {
        // Note: We don't use the value in a failure result, but we keep it to match the constructor signature.
        // In practice, we ignore the value when IsSuccess is false.
    }
}