using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace ServerLib.Transactions
{
    [Serializable]
    public class TxException : RemotingException, ISerializable
    {
    }
}