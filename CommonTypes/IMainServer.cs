namespace CommonTypes
{
    public interface IMainServer
    {
        int StartTransaction();

        void JoinTransaction(int txid, string endpoint);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);

        /* Function to give to the client the server list */
        string[] getServerList();

        /* Function to give to the client the all Server Status */
        bool getServerStatus();
    }
}