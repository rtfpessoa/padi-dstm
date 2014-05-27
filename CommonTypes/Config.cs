namespace CommonTypes
{
    public static class Config
    {
        public const int InvocationTimeout = 5000;

        public const int RemoteMainserverPort = 9999;
        public const string RemoteMainserverObjName = "MainServer";

        private const int RemoteServerPortRange = 2000;
        public const string RemoteServerObjName = "Server";

        public static readonly string RemoteMainserverUrl = "tcp://localhost:" + RemoteMainserverPort + "/" +
                                                            RemoteMainserverObjName;

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