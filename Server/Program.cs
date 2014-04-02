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
            TcpChannel channelServ = new TcpChannel(Config.REMOTE_SERVER_PORT);
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Server), Config.REMOTE_SERVER_OBJ_NAME, WellKnownObjectMode.Singleton);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}