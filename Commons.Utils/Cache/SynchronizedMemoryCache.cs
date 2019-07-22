using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Commons.Utils.Cache
{
    /// <summary>
    /// Adattato da https://michaelscodingspot.com/cache-implementations-in-csharp-net/
    /// </summary>
    public class SynchronizedMemoryCache<T> : ILazyCache<T>
    {
        private const double DefaultCacheDuration = 30000D;
        private const double DefaultCreationWaitTime = 5000D;

        private readonly IScopeProvider _scopeProvider;
        private readonly TimeSpan _cacheDuration;
        private readonly TimeSpan _cacheEntryLockTimeout;

        private readonly MemoryCache _cache = new MemoryCache(typeof(T).Name);
        private readonly MemoryCache _keys = new MemoryCache("keys");

        public SynchronizedMemoryCache(IScopeProvider scopeProvider, double duration,
            double creationTimeout = DefaultCreationWaitTime)
        {
            _scopeProvider = scopeProvider;
            _cacheDuration = TimeSpan.FromMilliseconds(duration);
            _cacheEntryLockTimeout = TimeSpan.FromMilliseconds(creationTimeout);
        }

        public SynchronizedMemoryCache(IScopeProvider scopeProvider) : this(scopeProvider, DefaultCacheDuration)
        {
        }

        public SynchronizedMemoryCache(double duration, double creationTimeout = DefaultCreationWaitTime) : this(
            new GlobalScope(), duration, creationTimeout)
        {
        }

        public SynchronizedMemoryCache() : this(new GlobalScope(), DefaultCacheDuration)
        {
        }

        public async Task<T> GetOrCreate(string inputKey, Func<Task<T>> createItem)
        {
            var key = _scopeProvider.ApplyCurrentScope(inputKey);
            T cacheEntry;

            if (_cache.TryGetValue(key, out cacheEntry)) return cacheEntry;

            var creationLock = await AcquireCacheEntryCreationLock(key);
            try
            {
                if (!_cache.TryGetValue(key, out cacheEntry))
                {
                    cacheEntry = await createItem();
                    _cache.Set(key, cacheEntry, DateTimeOffset.Now.Add(_cacheDuration));
                }
            }
            finally
            {
                creationLock.Release();
            }

            return cacheEntry;
        }

        private async Task<SemaphoreSlim> AcquireCacheEntryCreationLock(string key)
        {
            SemaphoreSlim keySemaphore;
            if (!_keys.TryGetValue(key, out keySemaphore))
            {
                var newSemaphore = new SemaphoreSlim(1, 1);
                do
                {
                    if (_keys.TryGetValue(key, out keySemaphore)) break;
                    keySemaphore = newSemaphore;
                } while (!_keys.Add(key, keySemaphore, DateTimeOffset.Now.Add(_cacheDuration)));
            }

            if (!await keySemaphore.WaitAsync(_cacheEntryLockTimeout))
                throw new Exception("Timeout waiting for cache");

            return keySemaphore;
        }
    }

    public static class MemoryCacheExtensions
    {
        public static bool TryGetValue<T>(this MemoryCache cache, string key, out T cacheEntry)
        {
            var entry = cache.Get(key);
            if (entry != null)
            {
                cacheEntry = (T)entry;
                return true;
            }

            cacheEntry = default(T);
            return false;
        }
    }
}