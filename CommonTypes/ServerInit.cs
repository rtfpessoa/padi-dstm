using System;
using System.Collections.Generic;

namespace CommonTypes
{
    [Serializable]
    public class ServerInit
    {
        public readonly KeyValuePair<int, bool> Backup;

        public readonly Dictionary<int, bool> FaultDetection;
        public readonly int Parent;

        public readonly int ServerCount;
        public readonly int Uuid;

        public readonly int Version;

        public ServerInit(int uuid, int version, int parent, Dictionary<int, bool> faultDetection, int serverCount)
        {
            Uuid = uuid;
            Version = version;
            Parent = parent;
            FaultDetection = faultDetection;
            ServerCount = serverCount;
        }
    }
}