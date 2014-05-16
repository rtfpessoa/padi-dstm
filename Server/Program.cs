using CommonTypes;
using CommonTypes.Transactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

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
            properties["port"] = Config.GetServerPort(serverInit.Uuid);
            properties["timeout"] = Config.NoInvocationTimeout;
            channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, false);
            RemotingServices.Marshal(server, Config.RemoteServerObjName);

            server.StartSplitLock();

            if (serverInit.FaultDetection.Count > 1)
            {
                var backup = serverInit.FaultDetection.Keys.Max();

                /* Get data from backup */
                var backupServer = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(backup));
                backupServer.StartSplitLock();
                ParticipantStatus status = backupServer.OnChild(serverInit.Uuid, serverInit.Version, serverInit.ServerCount);
                server.SetStatus(status);
                backupServer.EndSplitLock();
            }
            else if (serverInit.Parent != -1)
            {
                var parent = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(serverInit.Parent));
                parent.StartSplitLock();
                ParticipantStatus status = parent.OnChild(serverInit.Uuid, serverInit.Version, serverInit.ServerCount);
                server.SetStatus(status);
                parent.EndSplitLock();
            }

            foreach (KeyValuePair<int, bool> faultDetection in serverInit.FaultDetection)
            {
                var fd = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(faultDetection.Key));
                fd.OnReborn(serverInit.Uuid);
            }

            server.EndSplitLock();

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}