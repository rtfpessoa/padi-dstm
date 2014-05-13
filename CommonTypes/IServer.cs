﻿namespace CommonTypes
{
    public interface IServer
    {
        /// <exception cref="TxException"> </exception>
        /// <exception cref="WrongVersionException"></exception>
        int ReadValue(int version, int txid, int key);

        /// <exception cref="TxException"> </exception>
        /// <exception cref="WrongVersionException"></exception>
        void WriteValue(int version, int txid, int key, int value);

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

        void DumpState();

        void SetVersion(int version);

        int GetVersion();

        IStorage AddChild(int uid);
    }
}