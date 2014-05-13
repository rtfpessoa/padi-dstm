using System;
using System.Collections.Generic;
using CommonTypes;
using ServerLib.NameRegistry;
using ServerLib.Transactions;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private readonly Dictionary<int, RegistryEntry> _registry = new Dictionary<int, RegistryEntry>();
        private int _serverUidGenerator;
        private int _version;

        public void RemoveServer(int uid)
        {
            RegistryEntry entry;

            if (_registry.TryGetValue(uid, out entry))
            {
                entry.Active = false;
            }
        }

        public Dictionary<int, RegistryEntry> ListServers()
        {
            return _registry;
        }

        /* Give the Server List */

        public bool GetServerStatus()
        {
            bool result = true;

            foreach (var serverEntry in _registry)
            {
                var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverEntry.Key));
                result &= server.Status();
            }

            return result;
        }

        public ServerInit AddServer()
        {
            lock (this)
            {
                int uid = _serverUidGenerator++;
                int version = ++_version;
                int parent = -1;

                /* First server case */
                if (_registry.Count == 0)
                {
                    _registry.Add(uid, new RegistryEntry(parent, true));
                }
                else
                {
                    RegistryEntry entry;
                    if (_registry.TryGetValue(uid, out entry))
                    {
                        /* Enable the disabled child */
                        entry.Active = true;
                        parent = entry.Parent;
                    }
                    else
                    {
                        /* Create a child for each current server */
                        int serverUid = uid;
                        int registrySize = _registry.Count;
                        for (int i = 0; i < registrySize; i++)
                        {
                            _registry.Add(serverUid, new RegistryEntry(i, (serverUid == uid)));
                            
                            if (serverUid == uid)
                            {
                                parent = i;
                            }

                            serverUid++;
                        }
                    }

                    /* Update parent children and update parent version */
                    RegistryEntry parentEntry;
                    _registry.TryGetValue(uid, out parentEntry);
                    parentEntry.Children.Add(uid);
                }

                return new ServerInit(uid, version, parent);
            }
        }

        public int GetVersion()
        {
            return _version;
        }
    }
}