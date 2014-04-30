using System;

namespace CommonTypes
{
    [Serializable]
    public class RegistryEntry
    {
        public readonly int Parent;
        public bool Active;

        public RegistryEntry(int parent, bool active)
        {
            Parent = parent;
            Active = active;
        }
    }
}