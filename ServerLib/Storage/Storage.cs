﻿using System.Collections.Generic;
using System.Linq;

namespace ServerLib.Storage
{
    public class KeyValueStorage : IStorage
    {
        private readonly Dictionary<int, int> _keyStore = new Dictionary<int, int>();

        public int ReadValue(int key)
        {
            int value;

            _keyStore.TryGetValue(key, out value);

            return value;
        }

        public void WriteValue(int key, int value)
        {
            _keyStore[key] = value;
        }

        public bool HasValue(int key)
        {
            return _keyStore.ContainsKey(key);
        }

        public List<KeyValuePair<int, int>> GetValues()
        {
            return _keyStore.ToList();
        }
    }
}