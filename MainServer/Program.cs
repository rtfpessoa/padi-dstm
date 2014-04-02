using CommonTypes;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MainServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpChannel channelServ = new TcpChannel(Config.REMOTE_MAINSERVER_PORT);
            ChannelServices.RegisterChannel(channelServ, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MainServer), Config.REMOTE_MAINSERVER_OBJ_NAME, WellKnownObjectMode.Singleton);

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}