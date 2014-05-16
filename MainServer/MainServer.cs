using CommonTypes;
using CommonTypes.NameRegistry;
using ServerLib.NameRegistry;
using ServerLib.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer, INameRegistry
    {
        private readonly Dictionary<int, RegistryEntry> _registry = new Dictionary<int, RegistryEntry>();
        private readonly HashSet<int> _deadServers = new HashSet<int>();
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
                    var server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(serverEntry.Key));
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
                RegistryEntry entry;
                Dictionary<int, bool> faultDetection = new Dictionary<int, bool>();

                if (_deadServers.Count > 0)
                {
                    var serverId = _deadServers.Min();
                    _registry[serverId].Active = true;

                    _registry.TryGetValue(serverId, out entry);
                    parent = entry.Parent;

                    foreach (int fd in entry.FaultDetection)
                    {
                        faultDetection.Add(fd, _registry[fd].Active);
                    }


                    return new ServerInit(serverId, version, parent, faultDetection, _registry.Count);
                }

                int uid = _serverUidGenerator++;

                /* First server case */
                if (_registry.Count == 0)
                {
                    _registry.Add(uid, new RegistryEntry(parent, true));

                    return new ServerInit(uid, version, parent, faultDetection, _registry.Count);
                }

                if (_registry.TryGetValue(uid, out entry))
                {
                    /* Enable the disabled child */
                    _registry[uid].Active = true;
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

                _registry[parent].FaultDetection.Add(uid);
                _registry[uid].FaultDetection.Add(parent);

                faultDetection.Add(parent, _registry[parent].Active);

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
                            var faultDetection = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(fd));
                            faultDetection.OnFaultDetectionDeath(deadId);
                        }
                    }

                    var currentDirectory = Directory.GetCurrentDirectory();
                    var serverReleaseDir = currentDirectory + "\\..\\..\\..\\Server\\bin\\Release";
                    var serverDebugDir = currentDirectory + "\\..\\..\\..\\Server\\bin\\Debug";
                    var serverExe = "Server.exe";

                    if (Directory.Exists(serverReleaseDir) && Directory.GetFiles(serverReleaseDir).ToList().Exists(x => x.EndsWith(serverExe)))
                    {
                        Process.Start(serverReleaseDir + "\\" + serverExe);
                        return;
                    }

                    if (Directory.Exists(serverDebugDir) && Directory.GetFiles(serverDebugDir).ToList().Exists(x => x.EndsWith(serverExe)))
                    {
                        Process.Start(serverDebugDir + "\\" + serverExe);
                        return;
                    }
                }
            }
        }
    }
}