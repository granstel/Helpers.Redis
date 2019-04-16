using System;
using System.Threading.Tasks;
using GranSteL.Helpers.Redis.Extensions;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace GranSteL.Helpers.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _dataBase;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public RedisCacheService(IDatabase dataBase, JsonSerializerSettings serializerSettings = null)
        {
            _dataBase = dataBase;

            if (serializerSettings != null)
            {
                _serializerSettings = serializerSettings;
            }
        }

        public async Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            if (data == null) return false;

            var value = data.Serialize(_serializerSettings);

            return await _dataBase.StringSetAsync(key, value, timeOut);
        }

        public bool TryGet<T>(string key, out T data, bool throwException)
        {
            ValidateKey(key);

            var result = false;

            data = default;

            try
            {
                var value = _dataBase.StringGet(key).ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    data = value.Deserialize<T>(_serializerSettings);

                    result = true;
                }
            }
            catch(Exception)
            {
                if(throwException)
                    throw;
            }

            return result;
        }

        public bool Exists(string key)
        {
            ValidateKey(key);

            return _dataBase.KeyExists(key);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            ValidateKey(key);

            return await _dataBase.KeyDeleteAsync(key);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "The key should be specified");
            }
        }
    }
}
