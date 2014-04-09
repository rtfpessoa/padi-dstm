using CommonTypes;
using ServerLib.NameRegistry;
using ServerLib.Transactions;
using System.Collections.Generic;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private int serverUidGenerator = Config.REMOTE_SERVER_PORT;
        private Dictionary<int, string> registry = new Dictionary<int, string>();

        public int AddServer(string endpoint)
        {
            int uid = serverUidGenerator++;
            registry.Add(uid, endpoint);
            return uid;
        }

        public void RemoveServer(int uid)
        {
            registry.Remove(uid);
        }

        public Dictionary<int, string> ListServers()
        {
            return registry;
        }

        /* Give the Server List */

        public bool getServerStatus()
        {
            return true;
        }
    }
}