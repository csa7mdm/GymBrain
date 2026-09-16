namespace GymBrain.Application.Common.Exceptions;
public sealed class RequestLimitException(string message, int retryAfterMinutes) : Exception(message)
{
    public int RetryAfterMinutes { get; } = retryAfterMinutes;
}
