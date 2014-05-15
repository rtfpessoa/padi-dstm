using System;
using System.Runtime.Serialization;

namespace CommonTypes.NameRegistry
{
    [Serializable]
    public class WrongVersionException : ApplicationException
    {
        public WrongVersionException()
        {
        }

        public WrongVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}