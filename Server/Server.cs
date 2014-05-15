using CommonTypes;
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
        private readonly IParticipant _participant;
        private readonly int _serverId;
        private bool _isFrozen;
        private int _version;
        private readonly int _parent;
        private HashSet<int> _children;

        public Server(ServerInit serverInit)
        {
            _serverId = serverInit.GetUuid();
            _participant = new Participant(_serverId, new KeyValueStorage());
            _version = serverInit.GetVersion();
            _parent = serverInit.GetParent();
            _children = new HashSet<int>();
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
            return _isFrozen = true;
        }

        public bool Recover()
        {
            lock (this)
            {
                _isFrozen = false;

                Monitor.PulseAll(this);
            }

            return true;
        }

        public int ReadValue(int version, int txid, int key)
        {
            WaitIfFrozen();

            //if !my value

            int value = ParticipantReadValue(version, txid, key);
            try
            {
                GetReplica().ReadThrough(version, txid, key);
            }
            catch (NoReplicationAvailableException ignored) { }

            return value;
        }

        public void ReadThrough(int version, int txid, int key)
        {
            WaitIfFrozen();

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

            //if !my value

            ParticipantWriteValue(version, txid, key, value);
            try
            {
                GetReplica().WriteThrough(version, txid, key, value);
            }
            catch (NoReplicationAvailableException ignored) { }
        }

        public void WriteThrough(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

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

            _participant.PrepareTransaction(txid);
        }

        public void CommitTransaction(int txid)
        {
            WaitIfFrozen();

            _participant.CommitTransaction(txid);
        }

        public void AbortTransaction(int txid)
        {
            WaitIfFrozen();

            _participant.AbortTransaction(txid);
        }

        public void SetVersion(int version)
        {
            _version = version;
        }

        public int GetVersion()
        {
            return _version;
        }

        public IStorage AddChild(int uid)
        {
            WaitIfFrozen();

            _children.Add(uid);

            return _participant.GetStorage();
        }

        public void SetStorage(IStorage storage)
        {
            _participant.SetStorage(storage);
        }

        private void WaitIfFrozen()
        {
            if (_isFrozen)
            {
                Monitor.Wait(this);
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
    }
}