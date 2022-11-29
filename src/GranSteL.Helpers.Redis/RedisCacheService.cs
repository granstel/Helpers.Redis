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
        private readonly string _keyPrefix;

        private readonly JsonSerializerSettings _defaultSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public RedisCacheService(IDatabase dataBase, string keyPrefix = null, JsonSerializerSettings defaultSerializerSettings = null)
        {
            _dataBase = dataBase;
            _keyPrefix = keyPrefix;

            if (defaultSerializerSettings != null)
            {
                _defaultSerializerSettings = defaultSerializerSettings;
            }
        }

        public async Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            if (data == null) return false;

            var value = data.Serialize(_defaultSerializerSettings);

            key = GetFullKey(key);

            return await _dataBase.StringSetAsync(key, value, timeOut).ConfigureAwait(false);
        }

        public async Task<bool> TryAddAsync(string key, object data, TimeSpan? timeOut = null, bool throwException = false)
        {
            try
            {
                return await AddAsync(key, data, timeOut);
            }
            catch (NullKeyException)
            {
                throw;
            }
            catch (Exception)
            {
                if (throwException)
                    throw;

                return false;
            }
        }

        public bool Add(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            if (data == null) return false;

            var value = data.Serialize(_defaultSerializerSettings);

            key = GetFullKey(key);

            return _dataBase.StringSet(key, value, timeOut);
        }

        public bool TryAdd(string key, object data, TimeSpan? timeOut = null, bool throwException = false)
        {
            try
            {
                return Add(key, data, timeOut);
            }
            catch (NullKeyException)
            {
                throw;
            }
            catch (Exception)
            {
                if (throwException)
                    throw;

                return false;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var data = default(T);

            key = GetFullKey(key);

            var value = await _dataBase.StringGetAsync(key);

            var stringValue = value.ToString();

            if (!string.IsNullOrEmpty(stringValue))
            {
                data = stringValue.Deserialize<T>(_defaultSerializerSettings);
            }

            return data;
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var data = default(T);

            key = GetFullKey(key);

            var value = _dataBase.StringGet(key).ToString();

            if (!string.IsNullOrEmpty(value))
            {
                data = value.Deserialize<T>(_defaultSerializerSettings);
            }

            return data;
        }

        public bool TryGet<T>(string key, out T data, bool throwException = false)
        {
            data = default;

            try
            {
                ValidateKey(key);

                key = GetFullKey(key);

                var value = _dataBase.StringGet(key).ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    data = value.Deserialize<T>(_defaultSerializerSettings);

                    return true;
                }

                return false;
            }
            catch (NullKeyException)
            {
                throw;
            }
            catch (Exception)
            {
                if (throwException)
                    throw;
            }

            return false;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            ValidateKey(key);

            key = GetFullKey(key);

            return await _dataBase.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public bool Exists(string key)
        {
            ValidateKey(key);

            key = GetFullKey(key);

            return _dataBase.KeyExists(key);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            ValidateKey(key);

            key = GetFullKey(key);

            return await _dataBase.KeyDeleteAsync(key).ConfigureAwait(false);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullKeyException(nameof(key), "The key should be specified");
            }
        }

        private string GetFullKey(string key)
        {
            return $"{_keyPrefix}{key}";
        }
    }
}
