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
            IDictionary properties = new Hashtable();
            properties["timeout"] = "5000";
            properties["retryCount"] = "2";
            var channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, true);

            var mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            ServerInit serverInit = mainServer.AddServer();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);

            var server = new Server(serverInit);
            channelServ = new TcpChannel(Config.GetServerPort(serverInit.GetUuid()));
            ChannelServices.RegisterChannel(channelServ, true);
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