using System;

namespace CommonTypes
{
    [Serializable]
    public class ServerInit
    {
        private readonly int _uuid;

        private readonly int _version;

        public ServerInit(int uuid, int version)
        {
            _uuid = uuid;
            _version = version;
        }

        public int GetUuid()
        {
            return _uuid;
        }

        public int GetVersion()
        {
            return _version;
        }
    }
}