using System;
using System.Collections;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using CommonTypes.Transactions;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, false);

            var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);
            ServerInit serverInit = mainServer.AddServer();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);

            var server = new Server(serverInit);
            IDictionary properties = new Hashtable();
            int serverPort = Config.GetServerPort(serverInit.Uuid);
            const int serverTimeout = Config.InvocationTimeout;
            properties["port"] = serverPort;
            properties["timeout"] = serverTimeout;
            channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, false);
            RemotingServices.Marshal(server, Config.RemoteServerObjName);

            server.StartSplitLock();

            foreach (var faultDetection in serverInit.FaultDetection)
            {
                var fd = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(faultDetection.Key));
                fd.OnFaultDetectionReborn(serverInit.Uuid);
            }

            int backupId = -1;
            if ((serverInit.FaultDetection.Count == 1 && serverInit.FaultDetection.ContainsKey(serverInit.Parent) &&
                 serverInit.Parent != -1) || (serverInit.FaultDetection.Count == 0 && serverInit.Parent != -1))
            {
                backupId = serverInit.Parent;
            }
            else if (serverInit.FaultDetection.Count > 0)
            {
                backupId = serverInit.FaultDetection.Keys.Max();
            }

            if (backupId != -1)
            {
                /* Get data from backup */
                var backupServer = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(backupId));
                backupServer.StartSplitLock();
                ParticipantStatus status = backupServer.OnChild(serverInit.Uuid, serverInit.Version,
                    serverInit.ServerCount);
                server.SetStatus(status);
                backupServer.EndSplitLock();
            }
            else
            {
                Console.WriteLine("Server need to have a backup server! Is it the first one?");
            }

            server.EndSplitLock();

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}