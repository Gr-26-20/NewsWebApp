namespace NewsWebApp.Services
{
    public interface ISessionHelper
    {
        void Set<T>(string key, T value);
        T? Get<T>(string key);

        void Remove(string key);

        void Clear();
    }
}
