using CommonTypes;
using ServerLib.Storage;
using ServerLib.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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