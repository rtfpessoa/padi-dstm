using CommonTypes;
using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            TcpChannel channelServ = new TcpChannel(9997);
            ChannelServices.RegisterChannel(channelServ, true);

            IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);
            IServer server = (IServer)Activator.GetObject(typeof(IServer), Config.REMOTE_SERVER_URL);

            int txid1 = mainServer.StartTransaction();
            int txid2 = mainServer.StartTransaction();
            server.ReadValue(txid1, 1);
            server.WriteValue(txid1, 1, 10);
            server.ReadValue(txid2, 1);
            server.WriteValue(txid2, 1, 10);
            mainServer.CommitTransaction(txid1);
            mainServer.CommitTransaction(txid2);
        }
    }
}