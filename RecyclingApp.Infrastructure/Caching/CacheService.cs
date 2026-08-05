using Microsoft.Extensions.Caching.Distributed;
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

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedString = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedString))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cachedString);
        }
        catch (JsonException)
        {
            // If deserialization fails, return default rather than breaking the application flow
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
