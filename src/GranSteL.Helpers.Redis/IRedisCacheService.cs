using System;
using System.Threading.Tasks;

namespace GranSteL.Helpers.Redis
{
    public interface IRedisCacheService
    {
        Task<bool> AddAsync(string key, object data, TimeSpan? timeOut = null);

        bool TryGet<T>(string key, out T data, bool throwException = false);

        bool Exists(string key);

        Task<bool> DeleteAsync(string key);
    }
}