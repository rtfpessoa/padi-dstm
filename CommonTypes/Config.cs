namespace CommonTypes
{
    public static class Config
    {
        public static readonly int REMOTE_MAINSERVER_PORT = 9999;
        public static readonly string REMOTE_MAINSERVER_OBJ_NAME = "MainServer";
        public static readonly string REMOTE_MAINSERVER_URL = "tcp://localhost:" + REMOTE_MAINSERVER_PORT + "/" + REMOTE_MAINSERVER_OBJ_NAME;

        public static readonly int REMOTE_SERVER_PORT_RANGE = 9000;
        public static readonly string REMOTE_SERVER_OBJ_NAME = "Server";

        public static string GetServerUrl(int uid)
        {
            return "tcp://localhost:" + GetServerPort(uid) + "/" + REMOTE_SERVER_OBJ_NAME;
        }

        public static int GetServerPort(int uid)
        {
            return REMOTE_SERVER_PORT_RANGE + uid;
        }
    }
}