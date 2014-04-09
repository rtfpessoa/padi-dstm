namespace ServerLib.Transactions
{
    public interface ICoordinator
    {
        int StartTransaction();

        void JoinTransaction(int txid, int serveriId);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);
    }
}