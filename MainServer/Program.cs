using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using System.Collections;

namespace MainServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IDictionary properties = new Hashtable();
            properties["port"] = Config.RemoteMainserverPort;
            properties["timeout"] = 5000;
            properties["retryCount"] = 2;
            var channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MainServer), Config.RemoteMainserverObjName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}