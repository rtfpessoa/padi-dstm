using CommonTypes;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PADI_DSTM
{
    /*
    *   Lib created to be linked to every client application that uses PADI-DSTM 
    */
    public class PadiDstm
    {
        /* Variavel com o identificador da transacao actual */
        private static int currentTxInt;
        /* Variavel com a lista de servidores */
        private static string[] serverList;

        /*
        *      INTERACTION WITH SERVERS
        */

        /* 
        *   This method is called only once by the application and initializaes de PADI-DSTM library  
        */
        public static bool Init()
        {
            Console.WriteLine("[Client.Init] Entering Client.Init");

            TcpChannel channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, true);

            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal */
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

                /* 2. Temos que obter a list de servidores do sistema dada pelo MS */
                serverList = mainServer.getServerList();

                /* DEBUG PROPOSES*/
                for (int i = 0; i < serverList.Length; i++)
                {
                    Console.WriteLine("[Client.Init] Server {0}: {1}", i, serverList[i]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Init] Exception caught : {0}", e.StackTrace);
                Console.WriteLine("[Client.Init] Exiting Client.Init");
                return false;
            }

            Console.WriteLine("[Client.Init] Exiting Client.Init");
            return true;
        }

        /* 
        *   This method starts a new transaction and returns a boolean value indicating whether the
        *   operation succeeded. This method may throw a TxException
        */
        public static bool TxBegin()
        {
            Console.WriteLine("[Client.TxBegin] Entering Client.TxBegin");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

                /* 2. Chamar o metodo do servidor que dá inicio a transação */
                currentTxInt = mainServer.StartTransaction();

                /* DEBUG PROPOSES */
                Console.WriteLine("[Client.TxBegin] txInt: {0}", currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxBegin] Exception caught : {0}", e.StackTrace);
                Console.WriteLine("[Client.TxBegin] Exiting Client.TxBegin");
                return false;
            }

            Console.WriteLine("[Client.TxBegin] Exiting Client.TxBegin");
            return true;
        }

        /* 
        *   This method attempts to commit the current transaction and returns a boolean value
        *   indicating whether the operation succeded. This method may throw a TxException
        */
        public static bool TxCommit()
        {
            /* 1. Chamar o metodo do servidor que dá inicio ao commit da transação */
            Console.WriteLine("[Client.TxCommit] Entering Client.TxCommit");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

                /* 2. Chamar o metodo do servidor que dá inicio ao commit da transação */
                mainServer.CommitTransaction(currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxCommit] Exception caught : {0}", e.StackTrace);
                Console.WriteLine("[Client.TxCommit] Exiting Client.TxCommit");
                return false;
            }

            Console.WriteLine("[Client.TxCommit] Exiting Client.TxCommit");
            return true;
        }

        /* 
        *   This method aborts the current transaction and returns a boolean value indicating
        *   whether the operation succeeded. This method may throw a TxException
        */
        public static bool TxAbort()
        {
            /* 1. Chamar o metodo do servidor que aborta current transação*/
            Console.WriteLine("[Client.TxAbort] Entering Client.TxAbort");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                IMainServer mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);

                /* 2. Chamar o metodo do servidor que dá inicio ao commit da transação */
                mainServer.AbortTransaction(currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxAbort] Exception caught : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /* 
        *   This method makes all nodes in the system dump to their output their current state
        */
        public static bool Status()
        {
            Console.WriteLine("[Client.Status] Entering Status");

            bool status = true;
            bool srvStatus;

            try
            {
                /* Enviar a todos os servidores conhecidos a chamada ao metodo status */
                for (int i = 0; i < serverList.Length; i++)
                {
                    /* 1. Obter obj servidor */
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), serverList[i]);

                    /* 2. Chamada ao metodo */
                    srvStatus = server.Status();
                    status = status && srvStatus;

                    /* DEBUG PROPOSES */
                    Console.WriteLine("[Client.Status] Server{0} status: ", srvStatus);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Status] Exception : {0}", e.StackTrace);
                Console.WriteLine("[Client.Status] Exiting Status");
                return false;
            }

            Console.WriteLine("[Client.Status] Exiting Status");
            return status;
        }

        /* 
        *   This method makes the server at the URL stop responding to external calls except for
        *   a Recover call
        */
        public static bool Fail(String url)
        {
            Console.WriteLine("[Client.Fail] Entering Fail");

            bool fail = false;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                IServer server = (IServer)Activator.GetObject(typeof(IServer), url);

                /* 2. Chamar metodo presente nele que congela ele proprio */
                fail = server.Fail();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Fail] Exception : {0}", e.StackTrace);
                Console.WriteLine("[Client.Fail] Exiting Fail");
                return false;
            }

            Console.WriteLine("[Client.Fail] Exiting Fail");
            return fail;
        }

        /* 
        *   This method makes the server at URL stop responding to external calls but it
        *   maintains all calls for later reply, as if the communication to that server were
        *   only delayed. A server in freeze mode responds immediately only to a Recover 
        *   call (see below), which triggers the execution of the backlog of operations 
        *   accumulated since the Freeze call
        */
        public static bool Freeze(String url)
        {
            Console.WriteLine("[Client.Freeze] Entering Freeze");

            bool freeze = false;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                IServer server = (IServer)Activator.GetObject(typeof(IServer), url);

                /* 2. Chamar metodo presente nele que congela ele proprio */
                freeze = server.Freeze();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Freeze] Exception : {0}", e.StackTrace);
                Console.WriteLine("[Client.Freeze] Exiting Freeze");
                return false;
            }

            Console.WriteLine("[Client.Freeze] Exiting Freeze");
            return freeze;
        }

        /* 
        * This method creates a new shared object with the given uid. Returns null if the 
        * object already exists.
        */
        public static bool Recover(String url)
        {
            Console.WriteLine("[Client.Recover] Entering Recover");

            bool recover = false;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                IServer server = (IServer)Activator.GetObject(typeof(IServer), url);

                /* 2. Chamar metodo presente nele que renicia-o */
                recover = server.Recover();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Recover] Exception : {0}", e.StackTrace);
                Console.WriteLine("[Client.Recover] Exiting Recover");
                return false;
            }

            Console.WriteLine("[Client.Recover] Exiting Recover");
            return recover;
        }


        /*
        *      INTERACTION WITH SHARED DISTRIBUTED OBJECTS (PADINT)
        */

        /* 
        * This method creates a new shared object with the given uid. Returns null if 
        * the object already exists
        */
        public static PadInt CreatePadInt(int uid)
        {
            /* 1. Deverá ser feita a ligação ao servidor que pode ter o objecto (sabe-se isso através
             * da lista de servidores dada pelo Init() e pelo algoritmo de sharding implementado)
             */
            /* 2. Verificar se n tem e caso n tiver, cria-lo */
            /* Duvida? Como sabemos o que é um objecto PadInt? Criamos a classe onde?*/
            return null;
        }

        /* 
        * This method returns a reference to a shared object with the given uid. Returns 
        * null if the object does not exist already
        */
        public static PadInt AccessPadInt(int uid)
        {
            /* 1. Deverá ser feita a ligação ao servidor que pode ter o objecto (sabe-se isso através
             * da lista de servidores dada pelo Init() e pelo algoritmo de sharding implementado)
             */
            /* 2. Verificar se o tem e caso tiver, returnar uma referencia para ele */
            /* Duvida? Como sabemos o que é um objecto PadInt? Criamos a classe onde?*/
            return null;
        }
    }
}
