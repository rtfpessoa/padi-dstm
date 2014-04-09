using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;
using System;

namespace Server
{
    internal class Server : Participant, IServer
    {
        private static IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

        public Server()
            : base((ICoordinator)mainServer, new KeyValueStorage())
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