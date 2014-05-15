using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using System.Collections;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, false);

            var mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            ServerInit serverInit = mainServer.AddServer();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);

            var server = new Server(serverInit);
            IDictionary properties = new Hashtable();
            properties["port"] = Config.GetServerPort(serverInit.GetUuid());
            properties["timeout"] = 5000;
            channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, false);
            RemotingServices.Marshal(server, Config.RemoteServerObjName);

            if (serverInit.GetParent() != -1)
            {
                var parent = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(serverInit.GetParent()));
                parent.StartSplitLock();
                server.StartSplitLock();
                ParticipantStatus status = parent.AddChild(serverInit.GetUuid());
                server.SetStatus(status);
                parent.EndSplitLock();
                server.EndSplitLock();
            }

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}