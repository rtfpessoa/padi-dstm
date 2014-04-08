using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;
using System;

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