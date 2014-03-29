using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLib.Storage
{
    public class Storage : IStorage
    {
        private Dictionary<int, List<int>> transactionReadSet = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> transactionWriteSet = new Dictionary<int, List<int>>();
        private Dictionary<int, List<KeyValuePair<int, int>>> transactionPadInt = new Dictionary<int, List<KeyValuePair<int, int>>>();
        private Dictionary<int, int> padInts = new Dictionary<int, int>();

        public int ReadPadInt(int transaction, int uuid) {
            int value;
            
            if (!padInts.TryGetValue(uuid, out value))
            {
                value = 0;
            }

            return value;
        }

        public void WritePadInt(int transaction, int uuid, int value) {
            padInts[uuid] = value;
        }

        public Boolean PrepareTransaction(int transaction) { return true; }

        public Boolean CommitTransaction(int transaction) { return true; }

        public Boolean AbortTransaction(int transaction) { return true; }
    }
}
