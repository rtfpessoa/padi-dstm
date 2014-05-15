using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerLib.Transactions
{
    public class Participant : MarshalByRefObject, IParticipant
    {
        private readonly HashSet<int> _padIntLocks = new HashSet<int>();
        private readonly int _serverId;
        private readonly Dictionary<int, int> _startTxids = new Dictionary<int, int>();
        private IStorage _storage;

        private readonly Dictionary<int, Dictionary<int, int>> _txPadInts = new Dictionary<int, Dictionary<int, int>>();

        private readonly Dictionary<int, HashSet<int>> _txReadSet = new Dictionary<int, HashSet<int>>();
        private readonly Dictionary<int, HashSet<int>> _txWriteSet = new Dictionary<int, HashSet<int>>();

        private int _biggestCommitedTxid = -1;
        private ICoordinator _coordinator;

        public Participant(int serverId, IStorage storage)
        {
            _coordinator = (ICoordinator)Activator.GetObject(typeof(ICoordinator), Config.RemoteMainserverUrl);
            _serverId = serverId;
            _storage = storage;
        }

        public void PrepareTransaction(int txid)
        {
            if (IsReadOnlyTx(txid)) return;

            foreach (int padInt in _txWriteSet[txid])
            {
                lock (this)
                {
                    if (!_padIntLocks.Contains(padInt))
                    {
                        _padIntLocks.Add(padInt);
                    }
                    else
                    {
                        throw new TxException();
                    }
                }
            }

            if (ReadOtherWrites(txid))
            {
                foreach (int padInt in _txWriteSet[txid])
                {
                    _padIntLocks.Remove(padInt);
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
                foreach (var padint in _txPadInts[txid])
                {
                    _storage.WriteValue(padint.Key, padint.Value);
                }
            }

            foreach (int padInt in _txWriteSet[txid])
            {
                _padIntLocks.Remove(padInt);
            }

            lock (this)
            {
                if (txid > _biggestCommitedTxid)
                {
                    _biggestCommitedTxid = txid;
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
            foreach (var pair in _storage.GetValues())
            {
                Console.WriteLine("[Key:{0} | Value:{1}]", pair.Key, pair.Value);
            }
        }

        public int ReadValue(int txid, int key)
        {
            DoJoinTransaction(txid);

            int value;

            Dictionary<int, int> txPadInts;
            if (!_storage.HasValue(key) &&
                _txPadInts.TryGetValue(txid, out txPadInts) && !txPadInts.ContainsKey(key))
            {
                throw new TxException();
            }

            if (!_txReadSet.ContainsKey(txid))
            {
                _txReadSet[txid] = new HashSet<int>();
            }

            _txReadSet[txid].Add(key);

            if (_txPadInts.ContainsKey(txid) && _txPadInts[txid].ContainsKey(key))
            {
                _txPadInts[txid].TryGetValue(key, out value);
            }
            else
            {
                value = _storage.ReadValue(key);
            }

            Console.WriteLine("Tx {0} read the PadInt {1} with value {2}", txid, key, value);

            return value;
        }

        public void WriteValue(int txid, int key, int value)
        {
            DoJoinTransaction(txid);

            if (!_txWriteSet.ContainsKey(txid))
            {
                _txWriteSet[txid] = new HashSet<int>();
            }

            _txWriteSet[txid].Add(key);

            if (!_txPadInts.ContainsKey(txid))
            {
                _txPadInts[txid] = new Dictionary<int, int>();
            }

            _txPadInts[txid][key] = value;

            /* Fake read */
            ReadValue(txid, key);

            Console.WriteLine("Tx {0} wrote the PadInt {1} with value {2}", txid, key, value);
        }

        public IStorage GetStorage()
        {
            return _storage;
        }

        public void SetStorage(IStorage storage)
        {
            _storage = storage;
        }

        /*
         * Checks if a transaction is read-only (didn't wrote any PadInts)
         */

        private Boolean IsReadOnlyTx(int txid)
        {
            HashSet<int> writes;

            if (!_txWriteSet.TryGetValue(txid, out writes))
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
            int startTxid, endTxid = _biggestCommitedTxid;
            _startTxids.TryGetValue(txid, out startTxid);

            HashSet<int> reads;
            _txReadSet.TryGetValue(txid, out reads);

            for (int overlapTxid = startTxid + 1; overlapTxid <= endTxid; overlapTxid++)
            {
                if (!_txReadSet.ContainsKey(overlapTxid))
                {
                    HashSet<int> overlapTxWrites;
                    if (overlapTxid == txid || !_txWriteSet.TryGetValue(overlapTxid, out overlapTxWrites)) continue;

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
            _txWriteSet.TryGetValue(txid, out writes);

            var conflicts = new HashSet<int>();
            foreach (var overlapTx in _txReadSet)
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
            _txReadSet.Remove(txid);

            _txPadInts.Remove(txid);

            _startTxids.Remove(txid);

            if (isFail)
            {
                _txWriteSet.Remove(txid);
            }
        }

        private void DoJoinTransaction(int txid)
        {
            if (_startTxids.ContainsKey(txid)) return;

            _txPadInts.Add(txid, new Dictionary<int, int>());
            _txReadSet.Add(txid, new HashSet<int>());
            _txWriteSet.Add(txid, new HashSet<int>());

            _startTxids.Add(txid, _biggestCommitedTxid);
            GetCoordinator().JoinTransaction(txid, _serverId);
        }

        private ICoordinator GetCoordinator()
        {
            return _coordinator ??
                   (_coordinator = (ICoordinator)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl));
        }
    }
}