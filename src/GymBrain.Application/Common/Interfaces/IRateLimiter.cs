namespace GymBrain.Application.Common.Interfaces;

public interface IRateLimiter
{
    Task<(bool isExceeded, int retryAfterMinutes)> CheckLimitAsync(string userId, string endpoint, int limit, CancellationToken ct = default);
}
