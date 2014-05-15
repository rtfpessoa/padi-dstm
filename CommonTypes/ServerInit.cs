using System;

namespace CommonTypes
{
    [Serializable]
    public class ServerInit
    {
        private readonly int _uuid;

        private readonly int _version;

        private readonly int _parent;

        private readonly int _serverCount;

        public ServerInit(int uuid, int version, int parent, int serverCount)
        {
            _uuid = uuid;
            _version = version;
            _parent = parent;
            _serverCount = serverCount;
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

        public int GetServerCount()
        {
            return _serverCount;
        }
    }
}