using System;
using System.Threading;
using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;

namespace Server
{
    internal class Server : MarshalByRefObject, IServer, IParticipant
    {
        private readonly IParticipant _participant;
        private readonly int _serverId;
        private bool _isFrozen;

        public Server(int serverId)
        {
            _serverId = serverId;
            _participant = new Participant(serverId, new KeyValueStorage());
        }

        public void DumpState()
        {
            _participant.DumpState();
        }

        public bool Status()
        {
            Console.WriteLine("########## STATE DUMP ##########");
            Console.WriteLine("[Server: {0}] OK: {1}", _serverId, !_isFrozen);
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

        public int ReadValue(int txid, int key)
        {
            WaitIfFrozen();

            return _participant.ReadValue(txid, key);
        }

        public void WriteValue(int txid, int key, int value)
        {
            WaitIfFrozen();

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

        private void WaitIfFrozen()
        {
            if (_isFrozen)
            {
                Monitor.Wait(this);
            }
        }
    }
}