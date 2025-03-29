using StackExchange.Redis;
using System.Text;

namespace BaseProject.Services.Cache
{
    public class RedisService: ICacheService
    {
        private readonly Lazy<ConnectionMultiplexer> LazyConnection;
        private readonly IConfiguration _config;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConfiguration config, ILogger<RedisService> logger)
        {
            this._config = config;
            this._logger = logger;
            var endpoints = config.GetValue<string>("RedisCacheEndpoints");
            var pwd = config.GetValue<string>("RedisCachePassword");

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { endpoints },
                Password = pwd
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        private ConnectionMultiplexer Connection => LazyConnection.Value;

        private StackExchange.Redis.IDatabase RedisCache => Connection.GetDatabase();

        public string Load(string key)
        {
            string result = null;
            try
            {
                result = RedisCache.StringGet(key);
            }
            catch
            {

            }

            return result;
        }

        public T Load<T>(string key)
        {
            if (typeof(T) == typeof(String))
            {
                return (T)(object)Load(key);
            }

            T result = default(T);

            try
            {
                var rawValue = RedisCache.StringGet(key);

                result = DeserializeFromBinary<T>(rawValue);
            }
            catch (Exception ex)
            {
                result = default(T); //return null when there is an error (i.e.: when it is impossible to cast)
            }

            return result;
        }


        public bool Remove(string key)
        {
            try
            {
                RedisCache.KeyDelete(key);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Save<T>(T objectToStore, string key, int expireTimeInMinutes)
        {
            if (objectToStore is string)
            {
                try
                {
                    RedisCache.StringSet(key, objectToStore as string, TimeSpan.FromMinutes(expireTimeInMinutes));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Redis cache save error on key: " + key);
                    _logger.LogError(ex.ToString());
                    return false;
                }
            }
            else
            {
                byte[] serializedObject;
                try
                {
                    serializedObject = SerializeToBinary<T>(objectToStore);
                }
                catch (Exception ex)
                {
                    //throw new Exception("Redis Cache: Impossible to serialize " + objectToStore);
                    _logger.LogError(ex.ToString());
                    return false;
                }

                try
                {
                    RedisCache.StringSet(key, serializedObject, TimeSpan.FromMinutes(expireTimeInMinutes));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return false;
                }
            }

            return true;
        }
        public void Increment(string key, TimeSpan expirationTimeSpan)
        {
            RedisCache.StringIncrement(key, 1);
            RedisCache.KeyExpire(key, expirationTimeSpan);
        }

        public void Expire(string key, int expirationTimeSpan)
        {
            RedisCache.KeyExpire(key, TimeSpan.FromMinutes(expirationTimeSpan));
        }

        private static byte[] SerializeToBinary<TData>(TData data)
        {
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var encoded = Encoding.UTF8.GetBytes(serialized);
            return encoded;
        }

        private static TData DeserializeFromBinary<TData>(byte[] data)
        {
            var serialized = Encoding.UTF8.GetString(data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TData>(serialized);
        }
    }
}
