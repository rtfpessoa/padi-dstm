using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;

namespace Server
{
    internal class Server : Participant, IServer
    {
        public Server()
            : base(new KeyValueStorage())
        {
        }

        public bool Status()
        {
            return true;
        }

        public bool Fail()
        {
            return true;
        }

        public bool Freeze()
        {
            return true;
        }

        public bool Recover()
        {
            return true;
        }
    }
}