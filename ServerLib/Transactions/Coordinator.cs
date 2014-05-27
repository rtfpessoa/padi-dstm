using System;
using System.Collections.Generic;
using CommonTypes;
using CommonTypes.Transactions;

namespace ServerLib.Transactions
{
    public abstract class Coordinator : MarshalByRefObject, ICoordinator
    {
        private readonly Dictionary<int, List<ParticipantProxy>> _transactions =
            new Dictionary<int, List<ParticipantProxy>>();

        private int _currentTxid;

        /**
         * Creates a new random global transaction id
         */

        public int StartTransaction()
        {
            int txid = _currentTxid++;

            _transactions.Add(txid, new List<ParticipantProxy>());

            Console.WriteLine("Started transaction with TxID {0}", txid);
            return txid;
        }

        /**
         * Adds participant to distributed transaction
         */

        public void JoinTransaction(int txid, int serverId)
        {
            List<ParticipantProxy> participants;

            if (!_transactions.TryGetValue(txid, out participants))
            {
                participants = new List<ParticipantProxy>();
            }

            var participant = new ParticipantProxy(Config.GetServerUrl(serverId));
            participants.Add(participant);

            Console.WriteLine("Server {0} joined to transaction {1}", serverId, txid);
        }

        /**
         * Check if participant is ready to commit.
         */

        /**
         * Commits if all participants are ready to commit. Otherwise aborts.
         */

        public void CommitTransaction(int txid)
        {
            PrepareTransaction(txid);

            if (!IsReadyToCommit(txid))
            {
                AbortTransaction(txid);
                throw new TxException();
            }

            bool result = true;
            List<ParticipantProxy> participants;

            _transactions.TryGetValue(txid, out participants);

            if (participants != null)
                foreach (ParticipantProxy participant in participants)
                {
                    IPartitipantProxy proxy = participant.GetProxy();

                    try
                    {
                        proxy.CommitTransaction(txid);

                        Console.WriteLine("{0} commited transaction {1}", participant.Endpoint, txid);
                    }
                    catch (TxException ex)
                    {
                        result = false;

                        Console.WriteLine("{0} failed to commit transaction {1}", participant.Endpoint, txid);
                        Console.WriteLine(ex.ToString());

                        break;
                    }
                }

            if (!result)
            {
                AbortTransaction(txid);
                throw new TxException();
            }
        }

        public void AbortTransaction(int txid)
        {
            List<ParticipantProxy> participants;

            _transactions.TryGetValue(txid, out participants);

            if (participants == null) throw new TxException();

            var pending = new Queue<ParticipantProxy>(participants);

            while (pending.Count > 0)
            {
                ParticipantProxy proxy = pending.Dequeue();

                if (!AbortTransactionForParticipant(txid, proxy))
                {
                    pending.Enqueue(proxy);
                }
            }
        }

        private void PrepareTransaction(int txid)
        {
            List<ParticipantProxy> participants;

            _transactions.TryGetValue(txid, out participants);

            if (participants == null) return;

            foreach (ParticipantProxy participant in participants)
            {
                IPartitipantProxy proxy = participant.GetProxy();
                try
                {
                    proxy.PrepareTransaction(txid);
                    participant.ReadyToCommit = true;

                    Console.WriteLine("{0} prepared for transaction {1}", participant.Endpoint, txid);
                }
                catch (TxException ex)
                {
                    participant.ReadyToCommit = false;

                    Console.WriteLine("{0} failed to prepare transaction {1}", participant.Endpoint, txid);
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /**
         * Checks if all registered participants in the distributed transaction are ready to
         * commit.
         */

        private bool IsReadyToCommit(int txid)
        {
            bool result = true;
            List<ParticipantProxy> participants;

            _transactions.TryGetValue(txid, out participants);

            if (participants == null) return true;

            foreach (ParticipantProxy participant in participants)
            {
                result &= participant.ReadyToCommit;
            }

            return result;
        }

        private bool AbortTransactionForParticipant(int txid, ParticipantProxy participant)
        {
            IPartitipantProxy proxy = participant.GetProxy();

            try
            {
                proxy.AbortTransaction(txid);

                Console.WriteLine("{0} aborted transaction {1}", participant.Endpoint, txid);

                return true;
            }
            catch (TxException ex)
            {
                Console.WriteLine("{0} failed to abort transaction {1}", participant.Endpoint, txid);
                Console.WriteLine(ex.ToString());

                return false;
            }
        }
    }
}