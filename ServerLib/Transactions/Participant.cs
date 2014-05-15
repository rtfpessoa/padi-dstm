using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerLib.Transactions
{
    public class Participant : MarshalByRefObject, IParticipant
    {
        private readonly int _serverId;
        private ICoordinator _coordinator;

        private ParticipantStatus _status = new ParticipantStatus();

        public Participant(int serverId, IStorage storage)
        {
            _coordinator = (ICoordinator)Activator.GetObject(typeof(ICoordinator), Config.RemoteMainserverUrl);
            _serverId = serverId;
            _status.storage = storage;
        }

        public void PrepareTransaction(int txid)
        {
            if (IsReadOnlyTx(txid)) return;

            foreach (int padInt in _status.txWriteSet[txid])
            {
                lock (this)
                {
                    if (!_status.padIntLocks.Contains(padInt))
                    {
                        _status.padIntLocks.Add(padInt);
                    }
                    else
                    {
                        throw new TxException();
                    }
                }
            }

            if (ReadOtherWrites(txid))
            {
                foreach (int padInt in _status.txWriteSet[txid])
                {
                    _status.padIntLocks.Remove(padInt);
                }

                throw new TxException();
            }

            IEnumerable<int> conflicts = WriteOtherReads(txid);
            foreach (int conflict in conflicts)
            {
                // TODO: Abort transaction `conflict`
            }
        }

        public void CommitTransaction(int txid)
        {
            if (!IsReadOnlyTx(txid))
            {
                foreach (var padint in _status.txPadInts[txid])
                {
                    _status.storage.WriteValue(padint.Key, padint.Value);
                }
            }

            foreach (int padInt in _status.txWriteSet[txid])
            {
                _status.padIntLocks.Remove(padInt);
            }

            lock (this)
            {
                if (txid > _status.biggestCommitedTxid)
                {
                    _status.biggestCommitedTxid = txid;
                }
            }

            Clean(txid);
        }

        public void AbortTransaction(int txid)
        {
            Clean(txid, true);
        }

        public void DumpState()
        {
            foreach (var pair in _status.storage.GetValues())
            {
                Console.WriteLine("[Key:{0} | Value:{1}]", pair.Key, pair.Value);
            }
        }

        public int ReadValue(int txid, int key)
        {
            DoJoinTransaction(txid);

            int value;

            Dictionary<int, int> txPadInts;
            if (!_status.storage.HasValue(key) &&
                _status.txPadInts.TryGetValue(txid, out txPadInts) && !txPadInts.ContainsKey(key))
            {
                throw new TxException();
            }

            if (!_status.txReadSet.ContainsKey(txid))
            {
                _status.txReadSet[txid] = new HashSet<int>();
            }

            _status.txReadSet[txid].Add(key);

            if (_status.txPadInts.ContainsKey(txid) && _status.txPadInts[txid].ContainsKey(key))
            {
                _status.txPadInts[txid].TryGetValue(key, out value);
            }
            else
            {
                value = _status.storage.ReadValue(key);
            }

            Console.WriteLine("Tx {0} read the PadInt {1} with value {2}", txid, key, value);

            return value;
        }

        public void WriteValue(int txid, int key, int value)
        {
            DoJoinTransaction(txid);

            if (!_status.txWriteSet.ContainsKey(txid))
            {
                _status.txWriteSet[txid] = new HashSet<int>();
            }

            _status.txWriteSet[txid].Add(key);

            if (!_status.txPadInts.ContainsKey(txid))
            {
                _status.txPadInts[txid] = new Dictionary<int, int>();
            }

            _status.txPadInts[txid][key] = value;

            /* Fake read */
            ReadValue(txid, key);

            Console.WriteLine("Tx {0} wrote the PadInt {1} with value {2}", txid, key, value);
        }

        public ParticipantStatus GetStatus()
        {
            return _status;
        }

        public void SetStatus(ParticipantStatus status)
        {
            _status = status;

            foreach (int txid in _status.startTxids.Keys)
            {
                GetCoordinator().JoinTransaction(txid, _serverId);
            }
        }

        /*
         * Checks if a transaction is read-only (didn't wrote any PadInts)
         */

        private Boolean IsReadOnlyTx(int txid)
        {
            HashSet<int> writes;

            if (!_status.txWriteSet.TryGetValue(txid, out writes))
            {
                return true;
            }

            return writes.Count == 0;
        }

        /*
         * Checks if there are any changed PadInts between the transaction reads
         *  and all the overlaping transactions writes that already commited
         */

        private Boolean ReadOtherWrites(int txid)
        {
            int startTxid, endTxid = _status.biggestCommitedTxid;
            _status.startTxids.TryGetValue(txid, out startTxid);

            HashSet<int> reads;
            _status.txReadSet.TryGetValue(txid, out reads);

            for (int overlapTxid = startTxid + 1; overlapTxid <= endTxid; overlapTxid++)
            {
                if (!_status.txReadSet.ContainsKey(overlapTxid))
                {
                    HashSet<int> overlapTxWrites;
                    if (overlapTxid == txid || !_status.txWriteSet.TryGetValue(overlapTxid, out overlapTxWrites)) continue;

                    if (reads != null && overlapTxWrites.Intersect(reads).Any())
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

        private IEnumerable<int> WriteOtherReads(int txid)
        {
            HashSet<int> writes;
            _status.txWriteSet.TryGetValue(txid, out writes);

            var conflicts = new HashSet<int>();
            foreach (var overlapTx in _status.txReadSet)
            {
                if (writes != null && (overlapTx.Key != txid && overlapTx.Value.Intersect(writes).Any()))
                {
                    conflicts.Add(overlapTx.Key);
                }
            }

            return conflicts;
        }

        /*
         *  Removes all the traces of the transaction
         */

        private void Clean(int txid, Boolean isFail = false)
        {
            _status.txReadSet.Remove(txid);

            _status.txPadInts.Remove(txid);

            _status.startTxids.Remove(txid);

            if (isFail)
            {
                _status.txWriteSet.Remove(txid);
            }
        }

        private void DoJoinTransaction(int txid)
        {
            if (_status.startTxids.ContainsKey(txid)) return;

            _status.txPadInts.Add(txid, new Dictionary<int, int>());
            _status.txReadSet.Add(txid, new HashSet<int>());
            _status.txWriteSet.Add(txid, new HashSet<int>());

            _status.startTxids.Add(txid, _status.biggestCommitedTxid);
            GetCoordinator().JoinTransaction(txid, _serverId);
        }

        private ICoordinator GetCoordinator()
        {
            return _coordinator ??
                   (_coordinator = (ICoordinator)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl));
        }
    }
}