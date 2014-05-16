using System;
using System.Collections.Generic;

namespace CommonTypes.NameRegistry
{
    [Serializable]
    public class RegistryEntry
    {
        public readonly int Parent;
        public readonly HashSet<int> FaultDetection;
        public bool Active;

        public RegistryEntry(int parent, bool active)
        {
            Parent = parent;
            FaultDetection = new HashSet<int>();
            Active = active;
        }
    }
}