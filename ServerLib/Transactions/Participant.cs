using ServerLib.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLib.Transactions
{
    internal abstract class Participant : IParticipant
    {
        private IStorage storage;

        private Dictionary<int, List<int>> txReadSet = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> txWriteSet = new Dictionary<int, List<int>>();

        private Dictionary<int, List<KeyValuePair<int, int>>> txPadInts = new Dictionary<int, List<KeyValuePair<int, int>>>();
        
        private int biggestCommitedTxid = 0;
        private Dictionary<int, int> startTxids = new Dictionary<int, int>();

        public Participant(IStorage storage)
        {
            this.storage = storage;
        }

        public int ReadValue(int txid, int key)
        {
            /* TODO: Add to read set */ 
            
            return storage.ReadValue(key);
        }

        public void WriteValue(int txid, int key, int value)
        {
            /* TODO: Add to write set */

            /* TODO: Add/Update transaction PadInts */ 
            
            storage.WriteValue(key, value);
        }

        public void PrepareTransaction(int txid)
        {
            if (!isReadOnlyTx(txid))
            {
                /* TODO: Lock all PadInts */

                /* TODO: Verify if it reads others writes */

                /* TODO: Verify if it writes others reads */
                
                if (false)
                {
                    throw new TxException();
                }
            }
        }

        public void CommitTransaction(int txid)
        {
            /* TODO: Replace PadInt with the transaction ones */

            /* TODO: Release locks */
            
            if (false)
            {
                throw new TxException();
            }

            txReadSet.Remove(txid);

            if (biggestCommitedTxid < txid)
            {
                biggestCommitedTxid = txid;
            }

            startTxids.Remove(txid);
        }

        public void AbortTransaction(int txid)
        {
            if (false)
            {
                throw new TxException();
            }
        }

        private Boolean isReadOnlyTx(int txid)
        {
            List<int> writes;

            if(txWriteSet.TryGetValue(txid, out writes)) {
                return true;
            }

            return writes.Count == 0;
        }

        private List<int> readOtherWrites(int txid)
        {
            int startTxid, endTxid = biggestCommitedTxid;
            startTxids.TryGetValue(txid, out startTxid);

            List<int> reads;
            txReadSet.TryGetValue(txid, out reads);

            List<int> conflicts = new List<int>();
            for (int overlapTxid = startTxid; overlapTxid <= endTxid; overlapTxid++)
            {
                List<int> overlapTxWrites;
                if (txWriteSet.TryGetValue(overlapTxid, out overlapTxWrites))
                {
                    if (overlapTxWrites.Intersect(reads).Any())
                    {
                        conflicts.Add(overlapTxid);
                    }
                }
            }

            return conflicts;
        }

        private List<int> writeOtherReads(int txid)
        {
            List<int> writes;
            txWriteSet.TryGetValue(txid, out writes);

            List<int> conflicts = new List<int>();
            foreach (KeyValuePair<int, List<int>> overlapTx in txReadSet)
            {
                if (overlapTx.Key != txid && overlapTx.Value.Intersect(writes).Any())
                {
                    conflicts.Add(overlapTx.Key);
                }
            }

            return conflicts;
        }
    }
}