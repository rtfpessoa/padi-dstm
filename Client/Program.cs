using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            TcpChannel channelServ = new TcpChannel(9997);
            ChannelServices.RegisterChannel(channelServ, true);

            IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);
            int txid = mainServer.StartTransaction();

            IServer server = (IServer)Activator.GetObject(typeof(IServer), Config.REMOTE_SERVER_URL);
            server.ReadValue(txid, 1);
        }
    }
}