using System;
using System.Runtime.Serialization;

namespace GranSteL.Helpers.Redis
{
    [Serializable]
    public class NullKeyException : ArgumentNullException
    {
        public NullKeyException()
        {
        }

        public NullKeyException(string paramName) : base(paramName)
        {
        }

        public NullKeyException(string paramName, string message) : base(paramName, message)
        {
        }

        public NullKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
