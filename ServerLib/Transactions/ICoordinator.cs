namespace ServerLib.Transactions
{
    public interface ICoordinator
    {
        int StartTransaction();

        void JoinTransaction(int txid, string endpoint);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);
    }
}