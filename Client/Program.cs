using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            var channelServ = new TcpChannel(4739);
            ChannelServices.RegisterChannel(channelServ, true);

            Console.WriteLine("Waiting for init. Press to start:");
            Console.ReadLine();

            var server = (IServer) Activator.GetObject(typeof (IServer), Config.GetServerUrl(0));
            server.Recover();
            server.Status();

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);
        }
    }
}