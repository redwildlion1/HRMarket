using System.Collections.Concurrent;
using System.Text.Json;

namespace HRMarket.Configuration.Redis;

/// <summary>
/// In-memory implementation of IRedisService for development without Redis
/// </summary>
public class InMemoryRedisService : IRedisService
{
    private readonly ConcurrentDictionary<string, (string Value, DateTime? Expiry)> _store = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _hashes = new();
    private readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> _sets = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _lists = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static bool IsExpired(DateTime? expiry) => expiry.HasValue && expiry.Value < DateTime.UtcNow;

    public Task<string?> GetStringAsync(string key)
    {
        if (_store.TryGetValue(key, out var entry) && !IsExpired(entry.Expiry))
            return Task.FromResult<string?>(entry.Value);
        
        _store.TryRemove(key, out _);
        return Task.FromResult<string?>(null);
    }

    public Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        var expiry = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;
        _store[key] = (value, expiry);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string key)
    {
        return Task.FromResult(_store.TryRemove(key, out _));
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (_store.TryGetValue(key, out var entry) && !IsExpired(entry.Expiry))
            return Task.FromResult(true);
        
        _store.TryRemove(key, out _);
        return Task.FromResult(false);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var json = await GetStringAsync(key);
        return json == null ? null : JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return SetStringAsync(key, json, expiration);
    }

    public Task<Dictionary<string, string>> GetHashAsync(string key)
    {
        return Task.FromResult(_hashes.TryGetValue(key, out var hash) 
            ? new Dictionary<string, string>(hash) 
            : new Dictionary<string, string>());
    }

    public Task SetHashAsync(string key, Dictionary<string, string> values)
    {
        _hashes[key] = new ConcurrentDictionary<string, string>(values);
        return Task.CompletedTask;
    }

    public Task<string?> GetHashFieldAsync(string key, string field)
    {
        if (_hashes.TryGetValue(key, out var hash) && hash.TryGetValue(field, out var value))
            return Task.FromResult<string?>(value);
        return Task.FromResult<string?>(null);
    }

    public Task SetHashFieldAsync(string key, string field, string value)
    {
        _hashes.GetOrAdd(key, _ => new ConcurrentDictionary<string, string>())[field] = value;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteHashFieldAsync(string key, string field)
    {
        return Task.FromResult(_hashes.TryGetValue(key, out var hash) && hash.TryRemove(field, out _));
    }

    public Task<bool> AddToSetAsync(string key, string value)
    {
        return Task.FromResult(_sets.GetOrAdd(key, _ => []).Add(value));
    }

    public Task<bool> RemoveFromSetAsync(string key, string value)
    {
        return Task.FromResult(_sets.TryGetValue(key, out var set) && set.TryRemove(value));
    }

    public Task<bool> IsInSetAsync(string key, string value)
    {
        return Task.FromResult(_sets.TryGetValue(key, out var set) && set.Contains(value));
    }

    public Task<List<string>> GetSetMembersAsync(string key)
    {
        return Task.FromResult(_sets.TryGetValue(key, out var set) 
            ? set.ToList() 
            : []);
    }

    public Task<long> PushToListAsync(string key, string value)
    {
        var list = _lists.GetOrAdd(key, _ => new ConcurrentQueue<string>());
        list.Enqueue(value);
        return Task.FromResult((long)list.Count);
    }

    public Task<string?> PopFromListAsync(string key)
    {
        if (_lists.TryGetValue(key, out var list) && list.TryDequeue(out var value))
            return Task.FromResult<string?>(value);
        return Task.FromResult<string?>(null);
    }

    public Task<List<string>> GetListRangeAsync(string key, long start = 0, long stop = -1)
    {
        if (!_lists.TryGetValue(key, out var list))
            return Task.FromResult(new List<string>());
        
        var items = list.ToList();
        var end = stop == -1 ? items.Count : (int)Math.Min(stop + 1, items.Count);
        return Task.FromResult(items.Skip((int)start).Take(end - (int)start).ToList());
    }

    public Task<bool> SetExpirationAsync(string key, TimeSpan expiration)
    {
        if (!_store.TryGetValue(key, out var entry)) return Task.FromResult(false);
        _store[key] = (entry.Value, DateTime.UtcNow.Add(expiration));
        return Task.FromResult(true);
    }

    public Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        if (!_store.TryGetValue(key, out var entry) || !entry.Expiry.HasValue) return Task.FromResult<TimeSpan?>(null);
        var ttl = entry.Expiry.Value - DateTime.UtcNow;
        return Task.FromResult<TimeSpan?>(ttl.TotalSeconds > 0 ? ttl : null);
    }

    public Task<bool> DeleteMultipleAsync(params string[] keys)
    {
        foreach (var key in keys)
            _store.TryRemove(key, out _);
        return Task.FromResult(true);
    }

    public Task<Dictionary<string, string?>> GetMultipleAsync(params string[] keys)
    {
        var result = new Dictionary<string, string?>();
        foreach (var key in keys)
        {
            result[key] = _store.TryGetValue(key, out var entry) && !IsExpired(entry.Expiry) 
                ? entry.Value 
                : null;
        }
        return Task.FromResult(result);
    }

    public Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        return Task.FromResult(_store.Keys.Where(k => regex.IsMatch(k)).ToList());
    }

    public Task<long> IncrementAsync(string key, long value = 1)
    {
        var current = _store.TryGetValue(key, out var entry) && long.TryParse(entry.Value, out var num) 
            ? num 
            : 0;
        var newValue = current + value;
        _store[key] = (newValue.ToString(), null);
        return Task.FromResult(newValue);
    }

    public Task<long> DecrementAsync(string key, long value = 1)
    {
        return IncrementAsync(key, -value);
    }

    private class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, byte> _dict = new();
        public bool Add(T item) => _dict.TryAdd(item, 0);
        public bool TryRemove(T item) => _dict.TryRemove(item, out _);
        public bool Contains(T item) => _dict.ContainsKey(item);
        public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}