using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Common.Interfaces;

/// <summary>
/// Service interface for distributed caching (Redis).
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
