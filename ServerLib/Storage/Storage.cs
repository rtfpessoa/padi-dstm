using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLib.Storage
{
    public class KeyValueStorage : IStorage
    {
        private Dictionary<int, int> keyStore = new Dictionary<int, int>();

        public int ReadValue(int key)
        {
            int value = 0;

            keyStore.TryGetValue(key, out value);

            return value;
        }

        public void WriteValue(int key, int value)
        {
            keyStore[key] = value;
        }
    }
}