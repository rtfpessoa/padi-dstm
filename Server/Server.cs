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
        private int _serverId;

        protected override int serverId
        {
            get
            {
                return _serverId;
            }
        }

        private static IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

        public Server()
            : base((ICoordinator)mainServer, new KeyValueStorage())
        {
            _serverId = mainServer.AddServer();

            TcpChannel channelServ = new TcpChannel(Config.GetServerPort(serverId));
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Server), Config.GetServerUrl(serverId), WellKnownObjectMode.Singleton);
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