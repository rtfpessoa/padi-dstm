using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using PADI_DSTM;
using System.Collections;

namespace ClientConsole.Tests
{
    internal class TransactionsTests
    {
        public void Run()
        {
            IDictionary properties = new Hashtable();
            properties["port"] = "9997";
            properties["timeout"] = "5000";
            properties["retryCount"] = "2";
            var channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, true);

            Console.WriteLine("Waiting for init. Press to start:");
            Console.ReadLine();

            var mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            var server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(0));

            int txid1 = mainServer.StartTransaction();
            int txid2 = mainServer.StartTransaction();
            server.WriteValue(0, txid1, 1, 1);
            server.WriteValue(0, txid2, 1, 2);
            mainServer.CommitTransaction(txid1);
            mainServer.CommitTransaction(txid2);

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);
        }
    }

    internal class FreezeTests
    {
        public void Run()
        {
            IDictionary properties = new Hashtable();
            properties["port"] = "9997";
            properties["timeout"] = "5000";
            properties["retryCount"] = "2";
            var channelServ = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(channelServ, true);

            Console.WriteLine("Waiting for init. Press to start:");
            Console.ReadLine();

            var mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.RemoteMainserverUrl);
            var server = (IServer)Activator.GetObject(typeof(IServer), Config.GetServerUrl(0));

            int txid = mainServer.StartTransaction();
            server.WriteValue(0, txid, 1, 1);
            server.Freeze();
            server.WriteValue(0, txid, 1, 2);
            Console.WriteLine("Waiting for recover. Press to start:");
            Console.ReadLine();
            server.Recover();
            mainServer.CommitTransaction(txid);

            channelServ.StopListening(null);
            ChannelServices.UnregisterChannel(channelServ);
        }
    }

    internal class TeacherTests
    {
        public void Run()
        {
            bool res;

            PadiDstm.Init();

            res = PadiDstm.TxBegin();
            PadInt pi_a = PadiDstm.CreatePadInt(0);
            PadInt pi_b = PadiDstm.CreatePadInt(1);
            res = PadiDstm.TxCommit();

            res = PadiDstm.TxBegin();
            pi_a = PadiDstm.AccessPadInt(0);
            pi_b = PadiDstm.AccessPadInt(1);
            pi_a.Write(36);
            pi_b.Write(37);
            Console.WriteLine("a = " + pi_a.Read());
            Console.WriteLine("b = " + pi_b.Read());
            PadiDstm.Status();
            // The following 3 lines assume we have 2 servers: one at port 2001 and another at port 2002
            res = PadiDstm.Freeze("tcp://localhost:2001/Server");
            res = PadiDstm.Recover("tcp://localhost:2001/Server");
            res = PadiDstm.Fail("tcp://localhost:2002/Server");
            res = PadiDstm.TxCommit();
        }
    }
}