using System;
using System.Runtime.Serialization;

namespace CommonTypes
{
    [Serializable]
    public class TxException : ApplicationException
    {
        public TxException()
        {
        }

        public TxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}