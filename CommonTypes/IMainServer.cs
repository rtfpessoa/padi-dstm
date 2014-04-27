using System.Collections.Generic;

namespace CommonTypes
{
    public interface IMainServer
    {
        int StartTransaction();

        void JoinTransaction(int txid, int serverId);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);

        ServerInit AddServer();

        void RemoveServer(int serverId);

        List<int> ListServers();

        /* Function to give to the client the all Server Status */

        bool GetServerStatus();
    }
}