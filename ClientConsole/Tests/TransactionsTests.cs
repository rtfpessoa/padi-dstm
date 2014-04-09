using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    internal class TransactionsTests
    {
        public void run()
        {
            TcpChannel channelServ = new TcpChannel(9997);
            ChannelServices.RegisterChannel(channelServ, true);

            IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            IServer server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(0));

            int txid1 = mainServer.StartTransaction();
            int txid2 = mainServer.StartTransaction();
            server.WriteValue(txid1, 1, 1);
            server.ReadValue(txid1, 1);
            server.WriteValue(txid1, 1, 10);
            server.ReadValue(txid2, 1);
            server.WriteValue(txid2, 1, 10);
            mainServer.CommitTransaction(txid1);
            mainServer.CommitTransaction(txid2);

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);
        }
    }
}