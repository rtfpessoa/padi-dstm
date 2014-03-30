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

        private Dictionary<int, List<int>> transactionReadSet = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> transactionWriteSet = new Dictionary<int, List<int>>();
        private Dictionary<int, List<KeyValuePair<int, int>>> transactionPadInt = new Dictionary<int, List<KeyValuePair<int, int>>>();

        public Participant(IStorage storage)
        {
            this.storage = storage;
        }

        public int ReadValue(int key)
        {
            return storage.ReadValue(key);
        }

        public void WriteValue(int key, int value)
        {
            storage.WriteValue(key, value);
        }

        public void PrepareTransaction(string txid)
        {
            if (false)
            {
                throw new TxException();
            }
        }

        public void CommitTransaction(string txid)
        {
            if (false)
            {
                throw new TxException();
            }
        }

        public void AbortTransaction(string txid)
        {
            if (false)
            {
                throw new TxException();
            }
        }
    }
}