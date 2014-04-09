using System.Collections.Generic;

namespace ServerLib.Storage
{
    public class KeyValueStorage : IStorage
    {
        private readonly Dictionary<int, int> keyStore = new Dictionary<int, int>();

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