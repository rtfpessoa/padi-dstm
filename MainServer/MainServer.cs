using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommonTypes;
using CommonTypes.NameRegistry;
using ServerLib.NameRegistry;
using ServerLib.Transactions;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private readonly HashSet<int> _deadServers = new HashSet<int>();
        private readonly Dictionary<int, RegistryEntry> _registry = new Dictionary<int, RegistryEntry>();
        private int _serverUidGenerator;
        private int _version;

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
                if (serverEntry.Value.Active)
                {
                    var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverEntry.Key));
                    result &= server.Status();
                }
            }

            return result;
        }

        public ServerInit AddServer()
        {
            lock (this)
            {
                int version = ++_version;
                int parent = -1;
                var faultDetection = new Dictionary<int, bool>();
                RegistryEntry entry;

                if (_deadServers.Count > 0)
                {
                    int serverId = _deadServers.Min();
                    entry = _registry[serverId];
                    entry.Active = true;

                    parent = entry.Parent;

                    foreach (int fd in entry.FaultDetection)
                    {
                        faultDetection.Add(fd, _registry[fd].Active);
                    }

                    _deadServers.Remove(serverId);

                    return new ServerInit(serverId, version, parent, faultDetection, _registry.Count);
                }

                int uid = _serverUidGenerator++;

                /* First server case */
                if (_registry.Count == 0)
                {
                    _registry.Add(uid, new RegistryEntry(parent, true));

                    return new ServerInit(uid, version, parent, faultDetection, _registry.Count);
                }

                if (!_registry.TryGetValue(uid, out entry))
                {
                    /* Create a child for each current server */
                    int serverUid = uid;
                    int registrySize = _registry.Count;
                    for (int i = 0; i < registrySize; i++)
                    {
                        _registry.Add(serverUid, new RegistryEntry(i, false));
                        serverUid++;
                    }
                }

                entry = _registry[uid];
                entry.Active = true;
                parent = entry.Parent;
                entry.FaultDetection.Add(parent);
                faultDetection.Add(parent, _registry[parent].Active);

                _registry[parent].FaultDetection.Add(uid);

                return new ServerInit(uid, version, parent, faultDetection, _registry.Count);
            }
        }

        public int GetVersion()
        {
            return _version;
        }

        public void RemoveFaultDetection(int serverId, int failDetection)
        {
            _registry[serverId].FaultDetection.Remove(failDetection);
        }

        public void ReportDead(int reporterId, int deadId)
        {
            lock (this)
            {
                if (!_deadServers.Contains(deadId))
                {
                    Console.WriteLine("Server {0} reported dead!", deadId);
                    _deadServers.Add(deadId);

                    RegistryEntry entry = _registry[deadId];
                    entry.Active = false;

                    foreach (int fd in entry.FaultDetection)
                    {
                        if (fd != reporterId && _registry[fd].Active)
                        {
                            var faultDetection =
                                (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(fd));
                            faultDetection.OnFaultDetectionDeath(deadId);
                        }
                    }

                    ForceServerDeath(deadId);

                    /* Hack: ugly way to launch new server instance */
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string serverReleaseDir = currentDirectory + "\\..\\..\\..\\Server\\bin\\Release";
                    string serverDebugDir = currentDirectory + "\\..\\..\\..\\Server\\bin\\Debug";
                    string serverExe = "Server.exe";

                    if (Directory.Exists(serverReleaseDir) &&
                        Directory.GetFiles(serverReleaseDir).ToList().Exists(x => x.EndsWith(serverExe)))
                    {
                        Process.Start(serverReleaseDir + "\\" + serverExe);
                        return;
                    }

                    if (Directory.Exists(serverDebugDir) &&
                        Directory.GetFiles(serverDebugDir).ToList().Exists(x => x.EndsWith(serverExe)))
                    {
                        Process.Start(serverDebugDir + "\\" + serverExe);
                    }
                }
            }
        }

        private static void ForceServerDeath(int serverId)
        {
            try
            {
                var deadServer =
                    (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverId));
                deadServer.Fail();
            }
            catch
            {
            }
        }
    }
}