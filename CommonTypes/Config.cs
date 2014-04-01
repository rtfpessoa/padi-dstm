using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public static class Config
    {
        public static readonly int REMOTE_MAINSERVER_PORT = 9999;
        public static readonly string REMOTE_MAINSERVER_OBJ_NAME = "ChatServer";
        public static readonly string REMOTE_MAINSERVER_URL = "tcp://localhost:" + REMOTE_MAINSERVER_PORT + "/" + REMOTE_MAINSERVER_OBJ_NAME;
    }
}