using System;
using System.Collections.Generic;

namespace PC1MAINAITradingSystem.Database
{
    public class CacheManager
    {
        private Dictionary<string, object> _cache = new Dictionary<string, object>();

        public T GetOrSet<T>(string key, Func<T> getDataFunc)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = getDataFunc();
            }
            return (T)_cache[key];
        }

        public void Clear()
        {
            _cache.Clear();
        }

        // Additional methods for cache management...
    }
}