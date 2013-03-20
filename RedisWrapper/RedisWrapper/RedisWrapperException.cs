using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RedisWrapper
{
     [Serializable]
    public class RedisWrapperException : Exception
    {
        public RedisWrapperException() : base() { }

        public RedisWrapperException(string message)
            : base(message)
        {

        }
        public RedisWrapperException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public RedisWrapperException(string message, Exception innerException)
            : base(message, innerException) { }

        public RedisWrapperException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected RedisWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
