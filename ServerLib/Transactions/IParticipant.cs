namespace ServerLib.Transactions
{
    internal interface IParticipant
    {
        /// <exception cref="TxException"></exception>
        void PrepareTransaction(string txid);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(string txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(string txid);
    }
}