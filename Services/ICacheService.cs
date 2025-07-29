namespace casus_oyunu.Services
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        bool TryGet<T>(string key, out T? value);
        void Clear();
    }
} 