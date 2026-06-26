namespace GymBrain.Application.Common.Exceptions;

public class ManagedCapException(string message, int retryAfterHours) : Exception(message)
{
    public int RetryAfterHours { get; } = retryAfterHours;
}
