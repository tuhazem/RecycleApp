using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RecyclingApp.Application.Common.Interfaces;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Infrastructure.Caching;

/// <summary>
/// Cache service implementing ICacheService using IDistributedCache (configured with Redis).
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> logger;

    public CacheService(IDistributedCache distributedCache , ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        this.logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedString = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedString))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedString);
        }
        catch (Exception ex)
        {

            logger.LogWarning(ex, "Cache get failed for key '{CacheKey}'. Falling back to DB.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        else
        {
            // Fallback default expiration: 1 hour
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        }

        var serializedString = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedString, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
