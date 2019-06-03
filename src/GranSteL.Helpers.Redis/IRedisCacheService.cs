using System;
using System.Threading.Tasks;

namespace GranSteL.Helpers.Redis
{
    public interface IRedisCacheService
    {
        Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null);

        T Get<T>(string key);

        bool TryGet<T>(string key, out T data, bool throwException = false);

        bool Exists(string key);

        Task<bool> ExistsAsync(string key);

        Task<bool> DeleteAsync(string key);
    }
}