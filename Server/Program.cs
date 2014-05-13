using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, true);

            var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);
            ServerInit serverInit = mainServer.AddServer();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);

            var server = new Server(serverInit);
            channelServ = new TcpChannel(Config.GetServerPort(serverInit.GetUuid()));
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingServices.Marshal(server, Config.RemoteServerObjName);

            if (serverInit.GetParent() != -1)
            {
                var parent = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(serverInit.GetParent()));
                server.SetStorage(parent.AddChild(serverInit.GetUuid()));
            }

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}