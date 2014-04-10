using System;
using System.Threading;
using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;

namespace Server
{
    internal class Server : MarshalByRefObject, IServer
    {
        private readonly IParticipant _participant;
        private bool _isFrozen;

        public Server(int serverId)
        {
            _participant = new Participant(serverId, new KeyValueStorage());
        }

        public bool Status()
        {
            Console.WriteLine("[ServerStatus] Entering/Exiting Status");

            /* No futuro deve ir buscar o status do servidor ("OK", "Freeze", "Fail") */
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

                return true;
            }
        }

        public int ReadValue(int txid, int key)
        {
            lock (this)
            {
                WaitIfFrozen();

                return _participant.ReadValue(txid, key);
            }
        }

        public void WriteValue(int txid, int key, int value)
        {
            lock (this)
            {
                WaitIfFrozen();

                _participant.WriteValue(txid, key, value);
            }
        }

        public void PrepareTransaction(int txid)
        {
            lock (this)
            {
                WaitIfFrozen();

                _participant.PrepareTransaction(txid);
            }
        }

        public void CommitTransaction(int txid)
        {
            lock (this)
            {
                WaitIfFrozen();

                _participant.CommitTransaction(txid);
            }
        }

        public void AbortTransaction(int txid)
        {
            lock (this)
            {
                WaitIfFrozen();

                _participant.AbortTransaction(txid);
            }
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