using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Server
{
    internal class Server : Participant, IServer
    {
        private readonly int _serverId;

        protected override int serverId
        {
            get
            {
                return _serverId;
            }
        }

        private readonly IMainServer mainServer;

        public Server()
            : base(new KeyValueStorage())
        {
        }

        public bool Status()
        {
            Console.WriteLine("[ServerStatus] Entering/Exiting Status");

            /* No futuro deve ir buscar o status do servidor ("OK", "Freeze", "Fail") */
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