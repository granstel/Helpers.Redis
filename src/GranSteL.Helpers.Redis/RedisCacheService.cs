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

        private readonly Action<Exception> _logException;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public RedisCacheService(IDatabase dataBase, Action<Exception> logException = null, JsonSerializerSettings serializerSettings = null)
        {
            _dataBase = dataBase;

            _logException = logException;

            if (serializerSettings != null)
            {
                _serializerSettings = serializerSettings;
            }
        }

        public async Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            var result = false;

            if (data != null)
            {
                try
                {
                    var value = data.Serialize(_serializerSettings);

                    result = await _dataBase.StringSetAsync(key, value, timeOut);
                }
                catch (Exception e)
                {
                    _logException?.Invoke(e);
                }
            }

            return result;
        }

        public bool TryGet<T>(string key, out T data)
        {
            ValidateKey(key);

            var result = false;

            data = default(T);

            try
            {
                var value = _dataBase.StringGet(key).ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    data = value.Deserialize<T>(_serializerSettings);

                    result = true;
                }
            }
            catch (Exception e)
            {
                _logException?.Invoke(e);
            }

            return result;
        }

        public bool Exist(string key)
        {
            ValidateKey(key);

            var result = false;

            try
            {
                result = _dataBase.KeyExists(key);
            }
            catch (Exception e)
            {
                _logException?.Invoke(e);
            }

            return result;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            ValidateKey(key);

            var result = false;

            try
            {
                result = await _dataBase.KeyDeleteAsync(key);
            }
            catch (Exception e)
            {
                _logException?.Invoke(e);
            }

            return result;
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
