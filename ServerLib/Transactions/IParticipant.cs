namespace ServerLib.Transactions
{
    internal interface IParticipant
    {
        /// <exception cref="TxException"></exception>
        void PrepareTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);
    }
}