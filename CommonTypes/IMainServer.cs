using System.Collections.Generic;
using CommonTypes.NameRegistry;
using CommonTypes.Transactions;

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

        Dictionary<int, RegistryEntry> ListServers();

        bool GetServerStatus();

        int GetVersion();

        void ReportDead(int reporterId, int deadId);

        void RemoveFaultDetection(int serverId, int failDetection);
    }
}