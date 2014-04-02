using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IServer
    {
        int ReadValue(int txid, int key);

        void WriteValue(int txid, int key, int value);

        /// <exception cref="TxException"></exception>
        void PrepareTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);

        bool Status();

        bool Fail();

        bool Freeze();

        bool Recover();
    }
}