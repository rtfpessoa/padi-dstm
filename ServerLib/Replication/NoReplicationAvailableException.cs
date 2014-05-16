using System;
using System.Runtime.Serialization;

namespace ServerLib
{
    [Serializable]
    public class NoReplicationAvailableException : ApplicationException
    {
        public NoReplicationAvailableException()
        {
        }

        public NoReplicationAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}