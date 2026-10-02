using System.Text.Json;

namespace NewsWebApp.Services
{
    public class SessionHelper : ISessionHelper
    {
        private readonly ISession _session;

        public SessionHelper(IHttpContextAccessor ContextAccessor)
        {
            _session = ContextAccessor.HttpContext!.Session;
        }


        /**
         * Stores a value in the session with the specified key after serializing it to JSON. 
         *
         * @param key The key under which the value will be stored in the session.
         * @param value The value to store in the session.
         */
        /// <summary>
        /// Stores a value in the session with the specified key after serializing it to JSON.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="key">The key under which the value will be stored in the session.</param>
        /// <param name="value">The value to store in the session.</param>
        public void Set<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            _session.SetString(key, json);
        }

        /**
         * Retrieves a value from the session by its key and deserializes it to the specified type. 
         *
         * @param key The key of the value to retrieve from the session.
         * @return The deserialized value of type T, or default(T) if the key does not exist in the session.
         */
        /// <summary>
        /// Retrieves a value from the session by its key and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the value to retrieve from the session.</param>
        /// <returns>The deserialized value of type T, or default(T) if the key does not exist in the session.</returns>

        public T? Get<T>(string key)
        {
            var json = _session.GetString(key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }


        /// <summary>
        /// Removes a value from the session by its key.
        /// </summary>
        /// <param name="key">The key of the value to remove from the session.</param>
        public void Remove(string key)
        {
            _session.Remove(key);
        }

        /// <summary>
        /// Clears all values from the session.
        /// </summary>
        public void Clear()
        {
            _session.Clear();
        }
    }
}
