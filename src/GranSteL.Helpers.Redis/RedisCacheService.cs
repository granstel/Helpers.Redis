﻿using System;
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

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public RedisCacheService(IDatabase dataBase, string keyPrefix = null, JsonSerializerSettings serializerSettings = null)
        {
            _dataBase = dataBase;
            _keyPrefix = keyPrefix;

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

            key = $"{_keyPrefix}{key}";

            return await _dataBase.StringSetAsync(key, value, timeOut).ConfigureAwait(false);
        }

        public async Task<bool> TryAddAsync(string key, object data, TimeSpan? timeOut = null, bool throwException = false)
        {
            ValidateKey(key);

            var result = false;

            try
            {
                result = await AddAsync(key, data, timeOut);
            }
            catch (Exception)
            {
                if (throwException)
                    throw;
            }

            return result;
        }

        public bool Add(string key, object data, TimeSpan? timeOut = null)
        {
            ValidateKey(key);

            if (data == null) return false;

            var value = data.Serialize(_serializerSettings);

            key = $"{_keyPrefix}{key}";

            return _dataBase.StringSet(key, value, timeOut);
        }

        public bool TryAdd(string key, object data, TimeSpan? timeOut = null, bool throwException = false)
        {
            ValidateKey(key);

            var result = false;

            try
            {
                result = Add(key, data, timeOut);
            }
            catch (Exception)
            {
                if (throwException)
                    throw;
            }

            return result;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var data = default(T);

            key = $"{_keyPrefix}{key}";

            var value = await _dataBase.StringGetAsync(key);

            var stringValue = value.ToString();

            if (!string.IsNullOrEmpty(stringValue))
            {
                data = stringValue.Deserialize<T>(_serializerSettings);
            }

            return data;
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var data = default(T);

            key = $"{_keyPrefix}{key}";

            var value = _dataBase.StringGet(key).ToString();

            if (!string.IsNullOrEmpty(value))
            {
                data = value.Deserialize<T>(_serializerSettings);
            }

            return data;
        }

        public bool TryGet<T>(string key, out T data, bool throwException = false)
        {
            ValidateKey(key);

            var result = false;

            data = default;

            try
            {
                data = Get<T>(key);

                if(data != null)
                {
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

        public async Task<bool> ExistsAsync(string key)
        {
            ValidateKey(key);

            key = $"{_keyPrefix}{key}";

            return await _dataBase.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public bool Exists(string key)
        {
            ValidateKey(key);

            key = $"{_keyPrefix}{key}";

            return _dataBase.KeyExists(key);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            ValidateKey(key);

            key = $"{_keyPrefix}{key}";

            return await _dataBase.KeyDeleteAsync(key).ConfigureAwait(false);
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
