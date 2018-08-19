using System;
using System.Runtime.Caching;

namespace Commons.Utils
{
    //Example usage:
    //public bool Contains(string item)
    //{
    //    var cache = MemoryCache.Default;
    //    cache.GetExistingOrAdd(ItemKey, _expirationTime, RetrieveItem);
    //[...]
    public static class ObjectCacheExtensions
    {
        public static T GetExistingOrAdd<T>(this ObjectCache cache, string key, TimeSpan expiration, Func<T> getValue, string regionName = null)
            => cache.GetExistingOrAdd(key, DateTimeOffset.Now.Add(expiration), getValue);

        public static T GetExistingOrAdd<T>(this ObjectCache cache, string key, DateTimeOffset absoluteExpiration, Func<T> getValue,
                string regionName = null)
            => cache.GetExistingOrAdd(key, new CacheItemPolicy {AbsoluteExpiration = absoluteExpiration}, getValue, regionName);

        public static T GetExistingOrAdd<T>(this ObjectCache cache, string key, CacheItemPolicy policy, Func<T> getValue, string regionName = null)
        {
            var cachedValue = cache[key] ?? getValue();
            return (T) cache.AddOrGetExisting(key, cachedValue, policy, regionName);
        }
    }
}
