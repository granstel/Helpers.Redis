using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace GranSteL.Helpers.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private static readonly JsonSerializerOptions DefaultWriteOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = true
        };

        private static readonly JsonSerializerOptions DefaultReadOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly IDatabase _dataBase;
        private readonly string _keyPrefix;
        private readonly JsonSerializerOptions _readOptions;
        private readonly JsonSerializerOptions _writeOptions;

        public RedisCacheService(IDatabase dataBase, string keyPrefix)
            : this(dataBase, keyPrefix, null)
        {
        }

        public RedisCacheService(IDatabase dataBase, string keyPrefix = null, JsonSerializerOptions defaultSerializerOptions = null)
        {
            _dataBase = dataBase;
            _keyPrefix = keyPrefix;
            _readOptions = defaultSerializerOptions ?? DefaultReadOptions;
            _writeOptions = defaultSerializerOptions ?? DefaultWriteOptions;
        }

        public async Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            if (data == null) return false;

            var value = Serialize(data);

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

            var value = Serialize(data);

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
                data = Deserialize<T>(stringValue);
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
                data = Deserialize<T>(value);
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
                    data = Deserialize<T>(value);

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

        private string Serialize(object data) =>
            data is string raw ? raw : JsonSerializer.Serialize(data, data.GetType(), _writeOptions);

        private T Deserialize<T>(string value) =>
            value is T raw ? raw : JsonSerializer.Deserialize<T>(value, _readOptions);

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
