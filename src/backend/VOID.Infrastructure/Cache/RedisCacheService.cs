using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using VOID.Application.Abstractions.IServices.ICacheServices;

namespace VOID.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
    }

    public async Task<bool> ExistsAsync(
        string key, 
        CancellationToken ct = default)
    {
        var data = await _cache.GetStringAsync(key, ct);
        return !string.IsNullOrEmpty(data);
    }

    public async Task<T?> GetAsync<T>(
        string key, 
        CancellationToken ct = default)
    {
        var data = await _cache.GetStringAsync(key, ct);

        if (string.IsNullOrEmpty(data))
            return default;

        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task RemoveAsync(
        string key, 
        CancellationToken ct = default)
        => await _cache.RemoveAsync(key, ct);
    

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiry = null, 
        CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = 
                expiry ?? _defaultOptions.AbsoluteExpirationRelativeToNow
        };

        var jsonData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(
            key, 
            jsonData, 
            options, ct);
    }
}