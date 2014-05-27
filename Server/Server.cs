using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonTypes;
using CommonTypes.NameRegistry;
using CommonTypes.Transactions;
using ServerLib;
using ServerLib.Storage;
using ServerLib.Transactions;

namespace Server
{
    internal class Server : MarshalByRefObject, IServer, IPartitipantProxy
    {
        private const int HeartbeatTime = 10000;
        private readonly Dictionary<int, bool> _faultDetection;
        private readonly int _parent;

        private readonly IParticipant _participant;
        private readonly int _serverId;
        private bool _isFrozen;
        private bool _isSplitLocked;
        private int _serverCount;

        private Timer _timerReference;
        private bool _timerRunning;
        private int _version;

        public Server(ServerInit serverInit)
        {
            _serverId = serverInit.Uuid;
            _serverCount = serverInit.ServerCount;
            _participant = new Participant(_serverId, new KeyValueStorage());
            _version = serverInit.Version;
            _isSplitLocked = false;
            StartHearthBeat();

            _parent = serverInit.Parent;
            _faultDetection = serverInit.FaultDetection;
        }

        public void DumpState()
        {
            _participant.DumpState();
        }

        public bool Status()
        {
            Console.WriteLine("########## STATE DUMP ##########");
            Console.WriteLine("[Server: {0}] Version: {1} | OK: {2}", _serverId, _version, !_isFrozen);
            _participant.DumpState();
            Console.WriteLine("################################");

            return !_isFrozen;
        }

        public bool Fail()
        {
            Environment.Exit(0);

            return true;
        }

        public bool Freeze()
        {
            StopHearthBeat();

            return _isFrozen = true;
        }

        public bool Recover()
        {
            lock (this)
            {
                _isFrozen = false;

                StartHearthBeat();

                Monitor.PulseAll(this);
            }

            return true;
        }

        public int ReadValue(int version, int txid, int key)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            if (_version > version)
            {
                throw new WrongVersionException();
            }

            if (!ConsistentHashCalculator.IsMyPadInt(_serverCount, key, _serverId) &&
                CheckServer(ConsistentHashCalculator.GetServerIdForPadInt(_serverCount, key)))
            {
                throw new WrongVersionException();
            }

            int value = _participant.ReadValue(txid, key);

            try
            {
                GetReplica().ReadThrough(version, txid, key);
            }
            catch
            {
            }

            return value;
        }

        public void ReadThrough(int version, int txid, int key)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            _participant.ReadValue(txid, key);
        }

        public void WriteValue(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();


            if (_version > version)
            {
                throw new WrongVersionException();
            }

            if (!ConsistentHashCalculator.IsMyPadInt(_serverCount, key, _serverId) &&
                CheckServer(ConsistentHashCalculator.GetServerIdForPadInt(_serverCount, key)))
            {
                return;
            }

            _participant.WriteValue(txid, key, value);

            try
            {
                GetReplica().WriteThrough(version, txid, key, value);
            }
            catch
            {
            }
        }

        public void WriteThrough(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            _participant.WriteValue(txid, key, value);
        }

        public void PrepareTransaction(int txid)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            _participant.PrepareTransaction(txid);
        }

        public void CommitTransaction(int txid)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            _participant.CommitTransaction(txid);
        }

        public void AbortTransaction(int txid)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            _participant.AbortTransaction(txid);
        }

        public ParticipantStatus OnChild(int uid, int version, int serverCount)
        {
            WaitIfFrozen();

            if (_faultDetection.Count > 0 && _faultDetection.Keys.Max() < uid)
            {
                foreach (int fd in _faultDetection.Keys)
                {
                    if (uid != fd)
                    {
                        var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(fd));
                        server.RemoveFaultDetection(_serverId);

                        var master = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);
                        master.RemoveFaultDetection(fd, _serverId);
                    }
                }
            }
            else if (_faultDetection.Count == 0 && _parent != -1)
            {
                var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(_parent));
                server.RemoveFaultDetection(_serverId);

                var master = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);
                master.RemoveFaultDetection(_parent, _serverId);
            }

            _faultDetection[uid] = true;

            _serverCount = serverCount;
            _version = version;

            return _participant.GetStatus();
        }

        public void OnFaultDetectionDeath(int deadId)
        {
            WaitIfFrozen();

            if (_faultDetection.ContainsKey(deadId))
            {
                _faultDetection[deadId] = false;
            }
        }

        public void OnFaultDetectionReborn(int deadId)
        {
            WaitIfFrozen();

            if (_faultDetection.ContainsKey(deadId))
            {
                _faultDetection[deadId] = true;
            }
        }

        public void StartSplitLock()
        {
            _isSplitLocked = true;
        }

        public void EndSplitLock()
        {
            lock (this)
            {
                _isSplitLocked = false;

                Monitor.PulseAll(this);
            }
        }

        public bool AreYouAlive()
        {
            WaitIfFrozen();

            Console.WriteLine("[Server:{0}] I'm Alive" + DateTime.Now, _serverId);
            return true;
        }

        public void RemoveFaultDetection(int uid)
        {
            if (_faultDetection.ContainsKey(uid))
            {
                _faultDetection.Remove(uid);
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);
                mainServer.RemoveFaultDetection(_serverId, uid);
            }
        }

        public void SetStatus(ParticipantStatus storage)
        {
            _participant.SetStatus(storage);
        }

        private void WaitIfFrozen()
        {
            lock (this)
            {
                if (_isFrozen)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void WaitIfSplitLocked()
        {
            lock (this)
            {
                if (_isSplitLocked)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private IServer GetReplica()
        {
            if (_faultDetection.Count > 0)
            {
                int backup = _faultDetection.Keys.Max();

                if (_faultDetection[backup])
                {
                    return (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(backup));
                }
            }

            throw new NoReplicationAvailableException();
        }

        private void TimerTask(Object server)
        {
            if (!_timerRunning)
            {
                Console.WriteLine("Killing ping service ... " + DateTime.Now);
                _timerReference.Dispose();
                return;
            }

            if (_faultDetection.Count > 0)
            {
                foreach (var faultDetection in _faultDetection.ToList())
                {
                    if (!faultDetection.Value)
                    {
                        continue;
                    }

                    CheckServer(faultDetection.Key);
                }
            }
            else
            {
                Console.WriteLine("Nobody to ping ... " + DateTime.Now, _parent);
            }
        }

        private bool CheckServer(int serverUid)
        {
            try
            {
                Console.WriteLine("Sending ping to server:{0} ... " + DateTime.Now, serverUid);
                var backupServer = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverUid));
                backupServer.AreYouAlive();
            }
            catch
            {
                Console.WriteLine("Ping to server:{0} failed ... " + DateTime.Now, serverUid);
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                OnFaultDetectionDeath(serverUid);

                mainServer.ReportDead(_serverId, serverUid);

                return false;
            }

            return true;
        }

        private void StartHearthBeat()
        {
            _timerRunning = true;
            _timerReference = new Timer(TimerTask, this, HeartbeatTime, HeartbeatTime);
        }

        private void StopHearthBeat()
        {
            _timerRunning = false;
        }
    }
}