﻿using System;
using System.Collections.Generic;

namespace ServerLib.Transactions
{
    public class Coordinator : ICoordinator
    {
        private Dictionary<string, List<ParticipantProxy>> transactions = new Dictionary<string, List<ParticipantProxy>>();

        /**
         * Creates a new random global transaction id
         */

        public string StartTransaction()
        {
            string txid = Guid.NewGuid().ToString();

            transactions.Add(txid, new List<ParticipantProxy>());

            Console.WriteLine("Started transaction with TxID {0}", txid);
            return txid;
        }

        /**
         * Adds participant to distributed transaction
         */

        public void JoinTransaction(string txid, string endpoint)
        {
            List<ParticipantProxy> participants;

            if (!transactions.TryGetValue(txid, out participants))
            {
                participants = new List<ParticipantProxy>();
            }

            ParticipantProxy participant = new ParticipantProxy(endpoint);
            participants.Add(participant);

            Console.WriteLine("Server {0} joined to transaction {0}", endpoint, txid);
        }

        /**
         * Check if participant is ready to commit.
         */

        private void PrepareTransaction(string txid)
        {
            List<ParticipantProxy> participants;

            transactions.TryGetValue(txid, out participants);

            foreach (ParticipantProxy participant in participants)
            {
                IParticipant proxy = participant.GetProxy();
                try
                {
                    proxy.PrepareTransaction(txid);
                    participant.readyToCommit = true;

                    Console.WriteLine("{0} prepared for transaction {0}", participant.endpoint, txid);
                }
                catch (TxException ex)
                {
                    participant.readyToCommit = false;

                    Console.WriteLine("{0} failed to prepare transaction {0}", participant.endpoint, txid);
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /**
         * Checks if all registered participants in the distributed transaction are ready to
         * commit.
         */

        private bool IsReadyToCommit(string txid)
        {
            bool result = true;
            List<ParticipantProxy> participants;

            transactions.TryGetValue(txid, out participants);

            foreach (ParticipantProxy participant in participants)
            {
                result &= participant.readyToCommit;
            }

            return result;
        }

        /**
         * Commits if all participants are ready to commit. Otherwise aborts.
         */

        public void CommitTransaction(string txid)
        {
            PrepareTransaction(txid);

            if (!IsReadyToCommit(txid))
            {
                AbortTransaction(txid);
                return;
            }

            bool result = true;
            List<ParticipantProxy> participants;

            transactions.TryGetValue(txid, out participants);

            foreach (ParticipantProxy participant in participants)
            {
                IParticipant proxy = participant.GetProxy();

                try
                {
                    proxy.CommitTransaction(txid);
                    result = true;

                    Console.WriteLine("{0} commited transaction {0}", participant.endpoint, txid);
                }
                catch (TxException ex)
                {
                    result = false;

                    Console.WriteLine("{0} failed to commit transaction {0}", participant.endpoint, txid);
                    Console.WriteLine(ex.ToString());

                    break;
                }
            }

            if (!result)
            {
                AbortTransaction(txid);
            }
        }

        public void AbortTransaction(string txid)
        {
            List<ParticipantProxy> participants;

            transactions.TryGetValue(txid, out participants);

            Queue<ParticipantProxy> pending = new Queue<ParticipantProxy>(participants);

            while (pending.Count > 0)
            {
                ParticipantProxy proxy = pending.Dequeue();

                if (!AbortTransactionForParticipant(txid, proxy))
                {
                    pending.Enqueue(proxy);
                }
            }
        }

        private bool AbortTransactionForParticipant(string txid, ParticipantProxy participant)
        {
            IParticipant proxy = participant.GetProxy();

            try
            {
                proxy.AbortTransaction(txid);

                Console.WriteLine("{0} aborted transaction {0}", participant.endpoint, txid);

                return true;
            }
            catch (TxException ex)
            {
                Console.WriteLine("{0} failed to abort transaction {0}", participant.endpoint, txid);
                Console.WriteLine(ex.ToString());

                return false;
            }
        }
    }
}