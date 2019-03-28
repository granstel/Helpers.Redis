using Newtonsoft.Json;

namespace GranSteL.Helpers.Redis.Extensions
{
    public static class SerializationExtensions
    {
        public static string Serialize(this object obj, JsonSerializerSettings settings = null)
        {
            if (!(obj is string result))
            {
                result = JsonConvert.SerializeObject(obj, settings);
            }

            return result;
        }

        public static T Deserialize<T>(this object obj, JsonSerializerSettings settings = null)
        {
            T result = default(T);

            if (obj is T deserialize)
            {
                result = deserialize;
            }
            else if (obj is string serialized)
            {
                result = JsonConvert.DeserializeObject<T>(serialized, settings);
            }

            return result;
        }
    }
}
