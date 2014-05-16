using System;
using System.Collections.Generic;
using CommonTypes.Storage;

namespace CommonTypes.Transactions
{
    [Serializable]
    public class ParticipantStatus
    {
        public readonly HashSet<int> padIntLocks = new HashSet<int>();
        public readonly Dictionary<int, int> startTxids = new Dictionary<int, int>();

        public readonly Dictionary<int, Dictionary<int, int>> txPadInts = new Dictionary<int, Dictionary<int, int>>();

        public readonly Dictionary<int, HashSet<int>> txReadSet = new Dictionary<int, HashSet<int>>();
        public readonly Dictionary<int, HashSet<int>> txWriteSet = new Dictionary<int, HashSet<int>>();

        public int biggestCommitedTxid = -1;
        public IStorage storage;
    }
}