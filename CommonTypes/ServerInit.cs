using System;

namespace CommonTypes
{
    [Serializable]
    public class ServerInit
    {
        private readonly int _uuid;

        private readonly int _version;

        private readonly int _parent;

        public ServerInit(int uuid, int version, int parent)
        {
            _uuid = uuid;
            _version = version;
            _parent = parent;
        }

        public int GetUuid()
        {
            return _uuid;
        }

        public int GetVersion()
        {
            return _version;
        }

        public int GetParent()
        {
            return _parent;
        }
    }
}