using System;

namespace CommonTypes
{
    public interface IServer
    {
        /// <exception cref="NullReferenceException"> </exception>
        int ReadValue(int txid, int key);

        void WriteValue(int txid, int key, int value);

        /// <exception cref="TxException"> </exception>
        void PrepareTransaction(int txid);

        /// <exception cref="TxException"> </exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"> </exception>
        void AbortTransaction(int txid);

        bool Status();

        bool Fail();

        bool Freeze();

        bool Recover();
    }
}