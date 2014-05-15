using CommonTypes;
using CommonTypes.NameRegistry;
using ServerLib.NameRegistry;
using ServerLib.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private readonly Dictionary<int, RegistryEntry> _registry = new Dictionary<int, RegistryEntry>();
        private readonly HashSet<int> _deadServers = new HashSet<int>();
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
                var server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(serverEntry.Key));
                result &= server.Status();
            }

            return result;
        }

        public ServerInit AddServer()
        {
            lock (this)
            {
                int version = ++_version;
                int parent = -1;
                RegistryEntry entry;

                if (_deadServers.Count > 0)
                {
                    var serverId = _deadServers.Min();
                    _registry.TryGetValue(serverId, out entry);
                    entry.Active = true;

                    return new ServerInit(serverId, version, entry.Parent, _registry.Count);
                }

                int uid = _serverUidGenerator++;

                /* First server case */
                if (_registry.Count == 0)
                {
                    _registry.Add(uid, new RegistryEntry(parent, true));
                    return new ServerInit(uid, version, -1, _registry.Count);
                }

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

                return new ServerInit(uid, version, parent, _registry.Count);
            }
        }

        public int GetVersion()
        {
            return _version;
        }

        public void ReportDead(int uid)
        {
            Console.WriteLine("Server {0} reported dead!", uid);
            _deadServers.Add(uid);
            RegistryEntry entry;
            _registry.TryGetValue(uid, out entry);
            entry.Active = false;
        }
    }
}