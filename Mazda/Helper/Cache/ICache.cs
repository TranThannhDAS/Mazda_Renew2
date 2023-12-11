namespace Mazda.Helper.Cache
{
    public interface ICache
    {
        public bool CheckExitsCache(string key);
        public void SetCache(string key, string value);
        public string GetCache(string key);
        public void Clear(string key);
    }
}
