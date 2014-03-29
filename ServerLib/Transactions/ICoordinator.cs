namespace ServerLib.Transactions
{
    internal interface ICoordinator
    {
        string StartTransaction();

        void JoinTransaction(string txid, string endpoint);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(string txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(string txid);
    }
}