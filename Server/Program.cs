using CommonTypes;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpChannel channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, true);

            var mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            var serverId = mainServer.AddServer();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);
            channelServ = null;

            IServer server = new Server(serverId, mainServer);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}