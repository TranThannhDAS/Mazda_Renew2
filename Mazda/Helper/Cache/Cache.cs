using Microsoft.Extensions.Caching.Memory;

namespace Mazda.Helper.Cache
{
    public class Cache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public Cache(IMemoryCache _memoryCache) 
        {
            this._memoryCache = _memoryCache;
        }
        public bool CheckExitsCache(string key)
        {
            if (string.IsNullOrEmpty(_memoryCache.Get<string>(key)))
            {
                return false;
            }
            return true;
        }

        public void Clear(string key)
        {
            _memoryCache.Remove(key);
        }

        public string GetCache(string key)
        {
             string getValue1 = _memoryCache.Get<string>(key);
             return getValue1;
        }

        public void SetCache(string key, string value)
        {
            _memoryCache.Set<string>(key, value);
        }
        public void DisplayAllCacheEntries()
        {
          
        }
    }
}
