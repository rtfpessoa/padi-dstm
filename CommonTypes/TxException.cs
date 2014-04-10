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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}