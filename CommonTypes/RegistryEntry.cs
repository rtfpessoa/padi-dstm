using System;
using System.Collections.Generic;

namespace CommonTypes
{
    [Serializable]
    public class RegistryEntry
    {
        public readonly int Parent;
        public HashSet<int> Children;
        public bool Active;

        public RegistryEntry(int parent, bool active)
        {
            Parent = parent;
            Children = new HashSet<int>();
            Active = active;
        }
    }
}