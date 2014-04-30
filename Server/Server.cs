using System;
using System.Threading;
using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;

namespace Server
{
    internal class Server : MarshalByRefObject, IServer, IPartitipantProxy
    {
        private readonly IParticipant _participant;
        private readonly int _serverId;
        private bool _isFrozen;
        private int _version;

        public Server(int serverId, int version)
        {
            _serverId = serverId;
            _participant = new Participant(serverId, new KeyValueStorage());
            _version = version;
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
            /* Maibe contact master server? */

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

            if (_version != version)
            {
                throw new WrongVersionException();
            }

            return _participant.ReadValue(txid, key);
        }

        public void WriteValue(int version, int txid, int key, int value)
        {
            WaitIfFrozen();

            if (_version != version)
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

        private void WaitIfFrozen()
        {
            if (_isFrozen)
            {
                Monitor.Wait(this);
            }
        }
    }
}