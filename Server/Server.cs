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
        private readonly int _parent;
        private HashSet<int> _children;
        private bool _isSplitLocked;

        private bool _timerRunning;
        private Timer _timerReference;

        public Server(ServerInit serverInit)
        {
            _serverId = serverInit.GetUuid();
            _serverCount = serverInit.GetServerCount();
            _participant = new Participant(_serverId, new KeyValueStorage());
            _version = serverInit.GetVersion();
            _parent = serverInit.GetParent();
            _children = new HashSet<int>();
            _isSplitLocked = false;
            startHearthBeat();
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
            stopHearthBeat();

            return _isFrozen = true;
        }

        public bool Recover()
        {
            lock (this)
            {
                _isFrozen = false;

                startHearthBeat();

                Monitor.PulseAll(this);
            }

            return true;
        }

        public int ReadValue(int version, int txid, int key)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            if (!ConsistentHashCalculator.IsMyPadInt(_serverCount, key, _serverId))
            {
                // check if replica is dead
                // if its dead tell master
                return 0;
            }

            int value = ParticipantReadValue(version, txid, key);
            try
            {
                GetReplica().ReadThrough(version, txid, key);
            }
            catch (NoReplicationAvailableException) { }

            return value;
        }

        public void ReadThrough(int version, int txid, int key)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            ParticipantReadValue(version, txid, key);
        }

        private int ParticipantReadValue(int version, int txid, int key)
        {
            if (_version < version)
            {
                throw new WrongVersionException();
            }

            return _participant.ReadValue(txid, key);
        }

        public void WriteValue(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            if (!ConsistentHashCalculator.IsMyPadInt(_serverCount, key, _serverId))
            {
                // check if replica is dead
                // if its dead tell master
                return;
            }

            ParticipantWriteValue(version, txid, key, value);
            try
            {
                GetReplica().WriteThrough(version, txid, key, value);
            }
            catch (NoReplicationAvailableException) { }
        }

        public void WriteThrough(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

            WaitIfSplitLocked();

            ParticipantWriteValue(version, txid, key, value);
        }

        private void ParticipantWriteValue(int version, int txid, int key, int value)
        {
            if (_version < version)
            {
                throw new WrongVersionException();
            }

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

        public ParticipantStatus AddChild(int uid, int version)
        {
            WaitIfFrozen();

            _children.Add(uid);

            _serverCount = _serverCount + 1;

            _version = version;

            return _participant.GetStatus();
        }

        public void RemoveChild(int uid)
        {
            WaitIfFrozen();

            _children.Remove(uid);

            _serverCount = _serverCount - 1;
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
            if (_children.Count == 0)
            {
                if (_parent != -1)
                {
                    return (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(_parent));
                }
                else
                {
                    throw new NoReplicationAvailableException();
                }
            }

            int replicaServerId = _children.Max();
            return (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(replicaServerId));
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

            if (_children.Count > 0)
            {
                foreach (int child in _children)
                {
                    try
                    {
                        Console.WriteLine("Sending ping to child:{0} ... " + DateTime.Now.ToString(), child);
                        IServer backupServer = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(child));
                        backupServer.AreYouAlive();
                    }
                    catch
                    {
                        Console.WriteLine("Ping to child:{0} failed ... " + DateTime.Now.ToString(), child);
                        IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
                        RemoveChild(_parent);
                        mainServer.ReportDead(child);
                    }
                }
            }
            else if (_parent != -1)
            {
                try
                {
                    Console.WriteLine("Sending ping to parent:{0} ... " + DateTime.Now.ToString(), _parent);
                    IServer backupServer = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(_parent));
                    backupServer.AreYouAlive();
                }
                catch
                {
                    Console.WriteLine("Ping to parent:{0} failed ... " + DateTime.Now.ToString(), _parent);
                    IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
                    RemoveChild(_parent);
                    mainServer.ReportDead(_parent);
                }
            }
            else
            {
                Console.WriteLine("Nobody to ping ... " + DateTime.Now.ToString(), _parent);
            }
        }

        private void startHearthBeat()
        {
            _timerRunning = true;
            _timerReference = new Timer(new TimerCallback(TimerTask), this, _fiveSeconds, _fiveSeconds);
        }

        private void stopHearthBeat()
        {
            _timerRunning = false;
        }
    }
}
