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
/// Designed with graceful degradation when Redis is temporarily unreachable.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
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
            _logger.LogWarning(ex, "Cache get failed for key '{CacheKey}'. Falling back to database.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            var serializedString = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedString, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key '{CacheKey}'. Continuing without caching.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove failed for key '{CacheKey}'. Continuing without evicting.", key);
        }
    }
}
