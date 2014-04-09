using CommonTypes;
using ServerLib.NameRegistry;
using ServerLib.Transactions;
using System.Collections.Generic;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private int serverUidGenerator = 0;
        private List<int> registry = new List<int>();

        public int AddServer()
        {
            int uid = serverUidGenerator++;
            registry.Add(uid);
            return uid;
        }

        public void RemoveServer(int uid)
        {
            registry.Remove(uid);
        }

        public List<int> ListServers()
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