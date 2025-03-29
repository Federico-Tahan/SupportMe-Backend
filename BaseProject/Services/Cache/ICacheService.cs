namespace BaseProject.Services.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Saves the specified objeto.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto">The objeto.</param>
        /// <param name="key">The key.</param>
        /// <param name="expireTimeInMinutes">The expire time in minutes.</param>
        /// <returns></returns>
        bool Save<T>(T objectToStore, string key, int expireTimeInMinutes);

        /// <summary>
        /// Loads the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        T Load<T>(string key);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool Remove(string key);

        /// <summary>
        /// Increments a counter in the given key. The key expires after expirationTimeSpan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expirationTimeSpan"></param>
        void Increment(string key, TimeSpan expirationTimeSpan);
        void Expire(string key, int expirationTimeSpan);
    }
}
