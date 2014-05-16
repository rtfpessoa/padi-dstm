using CommonTypes.Transactions;

namespace ServerLib.Transactions
{
    public interface IPartitipantProxy
    {
        /// <exception cref="TxException"></exception>
        void PrepareTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);

        void DumpState();
    }
}