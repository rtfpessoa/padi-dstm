﻿using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace MainServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IDictionary properties = new Hashtable();
            properties["port"] = Config.RemoteMainserverPort;
            properties["timeout"] = Config.InvocationTimeout;
            var channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof (MainServer), Config.RemoteMainserverObjName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}