using System.Text.Json;
using StackExchange.Redis;

namespace HRMarket.Configuration.Redis;

/// <summary>
/// Generic Redis service for caching and data storage
/// </summary>
public interface IRedisService
{
    // String operations
    Task<string?> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);

    // Generic object operations (JSON serialization)
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    // Hash operations
    Task<Dictionary<string, string>> GetHashAsync(string key);
    Task SetHashAsync(string key, Dictionary<string, string> values);
    Task<string?> GetHashFieldAsync(string key, string field);
    Task SetHashFieldAsync(string key, string field, string value);
    Task<bool> DeleteHashFieldAsync(string key, string field);

    // Set operations
    Task<bool> AddToSetAsync(string key, string value);
    Task<bool> RemoveFromSetAsync(string key, string value);
    Task<bool> IsInSetAsync(string key, string value);
    Task<List<string>> GetSetMembersAsync(string key);

    // List operations
    Task<long> PushToListAsync(string key, string value);
    Task<string?> PopFromListAsync(string key);
    Task<List<string>> GetListRangeAsync(string key, long start = 0, long stop = -1);

    // Expiration management
    Task<bool> SetExpirationAsync(string key, TimeSpan expiration);
    Task<TimeSpan?> GetTimeToLiveAsync(string key);

    // Batch operations
    Task<bool> DeleteMultipleAsync(params string[] keys);
    Task<Dictionary<string, string?>> GetMultipleAsync(params string[] keys);

    // Pattern matching
    Task<List<string>> GetKeysByPatternAsync(string pattern);

    // Atomic operations
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
}

/// <summary>
/// Production-ready Redis service implementation with comprehensive error handling
/// </summary>
public class RedisService(
    IConnectionMultiplexer redis,
    ILogger<RedisService> logger)
    : IRedisService, IDisposable
{
    private readonly IDatabase _db = redis.GetDatabase();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    #region String Operations

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting string value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            await _db.StringSetAsync(key, value, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting string value for key: {Key}", key);
            throw;
        }
    }

    #endregion

    #region Generic Object Operations

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting object for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _db.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting object for key: {Key}", key);
            throw;
        }
    }

    #endregion

    #region Key Operations

    public async Task<bool> DeleteAsync(string key)
    {
        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking existence of key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> SetExpirationAsync(string key, TimeSpan expiration)
    {
        try
        {
            return await _db.KeyExpireAsync(key, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting expiration for key: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            return await _db.KeyTimeToLiveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }

    #endregion

    #region Hash Operations

    public async Task<Dictionary<string, string>> GetHashAsync(string key)
    {
        try
        {
            var entries = await _db.HashGetAllAsync(key);
            return entries.ToDictionary(
                e => e.Name.ToString(),
                e => e.Value.ToString()
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting hash for key: {Key}", key);
            return new Dictionary<string, string>();
        }
    }

    public async Task SetHashAsync(string key, Dictionary<string, string> values)
    {
        try
        {
            var entries = values.Select(kvp =>
                new HashEntry(kvp.Key, kvp.Value)).ToArray();
            await _db.HashSetAsync(key, entries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting hash for key: {Key}", key);
            throw;
        }
    }

    public async Task<string?> GetHashFieldAsync(string key, string field)
    {
        try
        {
            var value = await _db.HashGetAsync(key, field);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting hash field {Field} for key: {Key}", field, key);
            return null;
        }
    }

    public async Task SetHashFieldAsync(string key, string field, string value)
    {
        try
        {
            await _db.HashSetAsync(key, field, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting hash field {Field} for key: {Key}", field, key);
            throw;
        }
    }

    public async Task<bool> DeleteHashFieldAsync(string key, string field)
    {
        try
        {
            return await _db.HashDeleteAsync(key, field);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting hash field {Field} for key: {Key}", field, key);
            return false;
        }
    }

    #endregion

    #region Set Operations

    public async Task<bool> AddToSetAsync(string key, string value)
    {
        try
        {
            return await _db.SetAddAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding to set for key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> RemoveFromSetAsync(string key, string value)
    {
        try
        {
            return await _db.SetRemoveAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing from set for key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> IsInSetAsync(string key, string value)
    {
        try
        {
            return await _db.SetContainsAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking set membership for key: {Key}", key);
            return false;
        }
    }

    public async Task<List<string>> GetSetMembersAsync(string key)
    {
        try
        {
            var members = await _db.SetMembersAsync(key);
            return members.Select(m => m.ToString()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting set members for key: {Key}", key);
            return new List<string>();
        }
    }

    #endregion

    #region List Operations

    public async Task<long> PushToListAsync(string key, string value)
    {
        try
        {
            return await _db.ListRightPushAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error pushing to list for key: {Key}", key);
            return 0;
        }
    }

    public async Task<string?> PopFromListAsync(string key)
    {
        try
        {
            var value = await _db.ListRightPopAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error popping from list for key: {Key}", key);
            return null;
        }
    }

    public async Task<List<string>> GetListRangeAsync(string key, long start = 0, long stop = -1)
    {
        try
        {
            var values = await _db.ListRangeAsync(key, start, stop);
            return values.Select(v => v.ToString()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting list range for key: {Key}", key);
            return new List<string>();
        }
    }

    #endregion

    #region Batch Operations

    public async Task<bool> DeleteMultipleAsync(params string[] keys)
    {
        try
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            await _db.KeyDeleteAsync(redisKeys);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting multiple keys");
            return false;
        }
    }

    public async Task<Dictionary<string, string?>> GetMultipleAsync(params string[] keys)
    {
        try
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _db.StringGetAsync(redisKeys);

            var result = new Dictionary<string, string?>();
            for (int i = 0; i < keys.Length; i++)
            {
                result[keys[i]] = values[i].HasValue ? values[i].ToString() : null;
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting multiple keys");
            return new Dictionary<string, string?>();
        }
    }

    #endregion

    #region Pattern Matching

    public Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        try
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            return Task.FromResult(keys.Select(k => k.ToString()).ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting keys by pattern: {Pattern}", pattern);
            return Task.FromResult(new List<string>());
        }
    }

    #endregion

    #region Atomic Operations

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            return await _db.StringIncrementAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing key: {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        try
        {
            return await _db.StringDecrementAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error decrementing key: {Key}", key);
            return 0;
        }
    }

    #endregion

    public void Dispose()
    {
        redis.Dispose();
    }
}