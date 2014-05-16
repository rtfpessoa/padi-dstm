using CommonTypes;
using CommonTypes.NameRegistry;
using CommonTypes.Transactions;
using ServerLib;
using ServerLib.Storage;
using ServerLib.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Server
{
    internal class Server : MarshalByRefObject, IServer, IPartitipantProxy
    {
        private const int _fiveSeconds = 10000;

        private readonly IParticipant _participant;
        private readonly int _serverId;
        private int _serverCount;
        private bool _isFrozen;
        private int _version;
        private Dictionary<int, bool> _faultDetection;
        private int _parent;
        private bool _isSplitLocked;

        private bool _timerRunning;
        private Timer _timerReference;

        private bool _hasChildren;

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

            if (serverInit.FaultDetection.Count == 1 && serverInit.FaultDetection.ContainsKey(_parent))
            {
                _hasChildren = false;
            }
            else if (serverInit.FaultDetection.Count > 0)
            {
                _hasChildren = true;
            }
            else
            {
                _hasChildren = false;
            }
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
            /* Maybe contact master server? */

            throw new StackOverflowException();
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
                //GetReplica().ReadThrough(version, txid, key);
            }
            catch { }

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
               // GetReplica().WriteThrough(version, txid, key, value);
            }
            catch { }
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
            if (!_hasChildren && _parent != -1)
            {
                var parent = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(_parent));
                parent.RemoveFaultDetection(_serverId);
            }
            else if (_faultDetection.Count > 0)
            {
                foreach (int fd in _faultDetection.Keys)
                {
                    if (fd != uid)
                    {
                        var server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(fd));
                        server.RemoveFaultDetection(_serverId);
                    }
                }
            }

            return OnChildReborn(uid, version, serverCount);
        }

        public ParticipantStatus OnChildReborn(int uid, int version, int serverCount)
        {
            WaitIfFrozen();

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

        private IServer GetReplica()
        {
            if (_faultDetection.Count > 0)
            {
                var backup = _faultDetection.Keys.Max();

                if (_faultDetection[backup])
                {
                    return (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(backup));
                }
            }

            throw new NoReplicationAvailableException();
        }

        public bool AreYouAlive()
        {
            WaitIfFrozen();

            Console.WriteLine("[Server:{0}] I'm Alive" + DateTime.Now.ToString(), _serverId);
            return true;
        }

        private void TimerTask(Object server)
        {
            if (!_timerRunning)
            {
                Console.WriteLine("Killing ping service ... " + DateTime.Now.ToString());
                _timerReference.Dispose();
                return;
            }

            if (_faultDetection.Count > 0)
            {
                foreach (KeyValuePair<int, bool> faultDetection in _faultDetection.ToList())
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
                Console.WriteLine("Nobody to ping ... " + DateTime.Now.ToString(), _parent);
            }
        }

        private bool CheckServer(int serverUid)
        {
            try
            {
                Console.WriteLine("Sending ping to server:{0} ... " + DateTime.Now.ToString(), serverUid);
                IServer backupServer = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(serverUid));
                backupServer.AreYouAlive();
            }
            catch
            {
                Console.WriteLine("Ping to server:{0} failed ... " + DateTime.Now.ToString(), serverUid);
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);

                OnFaultDetectionDeath(serverUid);

                mainServer.ReportDead(_serverId, serverUid);

                return false;
            }

            return true;
        }

        private void StartHearthBeat()
        {
            _timerRunning = true;
            _timerReference = new Timer(new TimerCallback(TimerTask), this, _fiveSeconds, _fiveSeconds);
        }

        private void StopHearthBeat()
        {
            _timerRunning = false;
        }


        public void RemoveFaultDetection(int uid)
        {
            if (_faultDetection.ContainsKey(uid))
            {
                _faultDetection.Remove(uid);
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
                mainServer.RemoveFaultDetection(_serverId, uid);
            }
        }


        public void OnReborn(int faultDetection)
        {
            _faultDetection[faultDetection] = true;
        }
    }
}
