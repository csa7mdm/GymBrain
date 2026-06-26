using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Infrastructure.Services;

public class RateLimiter(ICacheService cache) : IRateLimiter
{
    public async Task<(bool isExceeded, int retryAfterMinutes)> CheckLimitAsync(string userId, string endpoint, int limit, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var hourKey = $"ratelimit:{userId}:{endpoint}:{now:yyyyMMddHH}";
        
        // Increment and set 1h TTL if new
        var count = await cache.IncrementAsync(hourKey, TimeSpan.FromHours(1), ct);
        
        if (count > limit)
        {
            // Calculate minutes until the next hour
            var nextHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
            var retryAfter = (int)(nextHour - now).TotalMinutes;
            return (true, Math.Max(1, retryAfter));
        }
        
        return (false, 0);
    }
}
