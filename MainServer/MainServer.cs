using System;
using System.Collections.Generic;
using CommonTypes;
using ServerLib.NameRegistry;
using ServerLib.Transactions;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private readonly List<int> _registry = new List<int>();
        private int _serverUidGenerator;

        public int AddServer()
        {
            int uid = _serverUidGenerator++;
            _registry.Add(uid);
            return uid;
        }

        public void RemoveServer(int uid)
        {
            _registry.Remove(uid);
        }

        public List<int> ListServers()
        {
            return _registry;
        }

        /* Give the Server List */

        public bool GetServerStatus()
        {
            bool result = true;

            foreach (int serverId in _registry)
            {
                var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverId));
                result &= server.Status();
            }

            return result;
        }
    }
}