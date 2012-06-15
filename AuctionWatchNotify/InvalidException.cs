using System;
using System.Collections.Generic;
using System.Linq;

namespace WatchNotify
{
    public class InvalidException : Exception
    {
        public readonly string theInvalidType;

        public InvalidException()
        {
            
        }
        public InvalidException(string message, string TheInvalidType)
            : base(message)
        {
            theInvalidType = TheInvalidType;
        }
        public InvalidException(string message, Exception innerException, string TheInvalidType)
            : base(message, innerException)
        {
            theInvalidType = TheInvalidType;
        }
        protected InvalidException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, string TheInvalidType)
            : base(info, context)
        {
            theInvalidType = TheInvalidType;
        }         
    }
}
