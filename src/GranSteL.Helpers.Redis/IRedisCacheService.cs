using System;
using System.Threading.Tasks;

namespace GranSteL.Helpers.Redis
{
    public interface IRedisCacheService
    {
        Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null);

        bool Add(string key, object data, TimeSpan? timeOut = null);

        Task<T> GetAsync<T>(string key);

        T Get<T>(string key);

        bool TryGet<T>(string key, out T data, bool throwException = false);

        Task<bool> ExistsAsync(string key);

        bool Exists(string key);

        Task<bool> DeleteAsync(string key);
    }
}