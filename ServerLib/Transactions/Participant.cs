using CommonTypes;
using ServerLib.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerLib.Transactions
{
    public abstract class Participant : MarshalByRefObject, IParticipant
    {
        private ICoordinator coordinator;

        private readonly IStorage storage;
        
        private readonly HashSet<int> padIntLocks = new HashSet<int>();
        private readonly Dictionary<int, int> startTxids = new Dictionary<int, int>();
        
        private readonly Dictionary<int, Dictionary<int, int>> txPadInts = new Dictionary<int, Dictionary<int, int>>();

        private readonly Dictionary<int, HashSet<int>> txReadSet = new Dictionary<int, HashSet<int>>();
        private readonly Dictionary<int, HashSet<int>> txWriteSet = new Dictionary<int, HashSet<int>>();

        private int biggestCommitedTxid = -1;

        public Participant(IStorage storage)
        {
            this.storage = storage;
            this.coordinator = (ICoordinator)Activator.GetObject(typeof(ICoordinator), Config.RemoteMainserverUrl);
        }

        protected abstract int serverId { get; }

        public void PrepareTransaction(int txid)
        {
            if (!isReadOnlyTx(txid))
            {
                foreach (int padInt in txWriteSet[txid])
                {
                    if (!padIntLocks.Contains(padInt))
                    {
                        padIntLocks.Add(padInt);
                    }
                    else
                    {
                        throw new TxException();
                    }
                }

                if (readOtherWrites(txid))
                {
                    foreach (int padInt in txWriteSet[txid])
                    {
                        padIntLocks.Remove(padInt);
                    }

                    throw new TxException();
                }

                HashSet<int> conflicts = writeOtherReads(txid);
                foreach (int conflict in conflicts)
                {
                    // TODO: Abort transaction `conflict`
                }
            }
        }

        public void CommitTransaction(int txid)
        {
            if (!isReadOnlyTx(txid))
            {
                foreach (var padint in txPadInts[txid])
                {
                    storage.WriteValue(padint.Key, padint.Value);
                }
            }

            foreach (int padInt in txWriteSet[txid])
            {
                padIntLocks.Remove(padInt);
            }

            if (biggestCommitedTxid < txid)
            {
                biggestCommitedTxid = txid;
            }

            clean(txid);
        }

        public void AbortTransaction(int txid)
        {
            clean(txid, true);
        }

        public int ReadValue(int txid, int key)
        {
            DoJoinTransaction(txid);

            int value;

            if (!storage.HasValue(key))
            {
                throw new TxException();
            }

            if (!txReadSet.ContainsKey(txid))
            {
                txReadSet[txid] = new HashSet<int>();
            }

            txReadSet[txid].Add(key);

            if (txPadInts.ContainsKey(txid) && txPadInts[txid].ContainsKey(key))
            {
                txPadInts[txid].TryGetValue(key, out value);
            }
            else
            {
                value = storage.ReadValue(key);
            }

            Console.WriteLine("Tx {0} read the PadInt {1} with value {2}", txid, key, value);

            return value;
        }

        public void WriteValue(int txid, int key, int value)
        {
            DoJoinTransaction(txid);

            if (!txWriteSet.ContainsKey(txid))
            {
                txWriteSet[txid] = new HashSet<int>();
            }

            txWriteSet[txid].Add(key);

            if (!txPadInts.ContainsKey(txid))
            {
                txPadInts[txid] = new Dictionary<int, int>();
            }

            txPadInts[txid][key] = value;

            storage.WriteValue(key, value);

            /* Fake read */
            ReadValue(txid, key);

            Console.WriteLine("Tx {0} wrote the PadInt {1} with value {2}", txid, key, value);
        }

        /*
         * Checks if a transaction is read-only (didn't wrote any PadInts)
         */

        private Boolean isReadOnlyTx(int txid)
        {
            HashSet<int> writes;

            if (!txWriteSet.TryGetValue(txid, out writes))
            {
                return true;
            }

            return writes.Count == 0;
        }

        /*
         * Checks if there are any changed PadInts between the transaction reads
         *  and all the overlaping transactions writes that already commited
         */

        private Boolean readOtherWrites(int txid)
        {
            int startTxid, endTxid = biggestCommitedTxid;
            startTxids.TryGetValue(txid, out startTxid);

            HashSet<int> reads;
            txReadSet.TryGetValue(txid, out reads);

            for (int overlapTxid = startTxid + 1; overlapTxid <= endTxid; overlapTxid++)
            {
                HashSet<int> overlapTxWrites;
                if (overlapTxid != txid && txWriteSet.TryGetValue(overlapTxid, out overlapTxWrites))
                {
                    if (overlapTxWrites.Intersect(reads).Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
         * Checks if there are any changed PadInts between the transaction writes
         *  and all the overlaping transactions reads that still active
         */

        private HashSet<int> writeOtherReads(int txid)
        {
            HashSet<int> writes;
            txWriteSet.TryGetValue(txid, out writes);

            var conflicts = new HashSet<int>();
            foreach (var overlapTx in txReadSet)
            {
                if (overlapTx.Key != txid && overlapTx.Value.Intersect(writes).Any())
                {
                    conflicts.Add(overlapTx.Key);
                }
            }

            return conflicts;
        }

        /*
         *  Removes all the traces of the transaction
         */

        private void clean(int txid, Boolean isFail = false)
        {
            txReadSet.Remove(txid);

            txPadInts.Remove(txid);

            startTxids.Remove(txid);

            if (isFail)
            {
                txWriteSet.Remove(txid);
            }
        }

        private void DoJoinTransaction(int txid)
        {
            if (!startTxids.ContainsKey(txid))
            {
                txPadInts.Add(txid, new Dictionary<int, int>());
                txReadSet.Add(txid, new HashSet<int>());
                txWriteSet.Add(txid, new HashSet<int>());

                startTxids.Add(txid, biggestCommitedTxid);
                GetCoordinator().JoinTransaction(txid, serverId);
            }
        }

        private ICoordinator GetCoordinator()
        {
            if (coordinator == null)
            {
                coordinator = (ICoordinator)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            }

            return coordinator;
        }
    }
}