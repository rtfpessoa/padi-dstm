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

            //channelServ = new TcpChannel(Config.GetServerPort(serverId));
            //ChannelServices.RegisterChannel(channelServ, true);
            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(Server), Config.RemoteServerObjName, WellKnownObjectMode.Singleton);

            /* Register Object With Constructor */
            var server = new Server(serverId);
            channelServ = new TcpChannel(Config.GetServerPort(serverId));
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingServices.Marshal(server, Config.RemoteServerObjName);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}