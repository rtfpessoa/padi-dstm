namespace CommonTypes
{
    public static class Config
    {
        public static readonly int InvocationTimeout = 5000;
        public static readonly int NoInvocationTimeout = -1;
        
        public static readonly int RemoteMainserverPort = 9999;
        public static readonly string RemoteMainserverObjName = "MainServer";

        public static readonly string RemoteMainserverUrl = "tcp://localhost:" + RemoteMainserverPort + "/" +
                                                            RemoteMainserverObjName;

        public static readonly int RemoteServerPortRange = 2000;
        public static readonly string RemoteServerObjName = "Server";

        public static string GetServerUrl(int uid)
        {
            return "tcp://localhost:" + GetServerPort(uid) + "/" + RemoteServerObjName;
        }

        public static int GetServerPort(int uid)
        {
            return RemoteServerPortRange + uid;
        }
    }
}