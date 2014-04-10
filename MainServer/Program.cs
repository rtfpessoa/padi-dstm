using System;
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
            var channelServ = new TcpChannel(Config.RemoteMainserverPort);
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof (MainServer), Config.RemoteMainserverObjName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}