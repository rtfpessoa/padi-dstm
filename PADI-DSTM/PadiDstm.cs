using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace PADI_DSTM
{
    /*
     * Lib created to be linked to every client application that uses PADI-DSTM
     */

    public class PadiDstm
    {
        /* Variavel com o identificador da transacao actual */
        private static int _currentTxInt;
        /* Variavel com a lista de servidores */
        private static List<int> _serverList = new List<int>();

        /*
         * INTERACTION WITH SERVERS
         */

        /*
         * This method is called only once by the application and initializaes de PADI-DSTM library
         */

        public static bool Init()
        {
            var channelServ = new TcpChannel();
            ChannelServices.RegisterChannel(channelServ, true);

            Console.WriteLine("[Client.Init] Entering Client.Init");

            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal */
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                /* 2. Temos que obter a list de servidores do sistema dada pelo MS */
                _serverList = mainServer.ListServers();

                /* DEBUG PROPOSES*/
                for (var i = 0; i < _serverList.Count; i++)
                {
                    Console.WriteLine("[Client.Init] Server {0}: {1}", i, _serverList[i]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Init] Exception caught : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /*
         * This method starts a new transaction and returns a boolean value indicating whether the
         * operation succeeded. This method may throw a TxException
         */

        public static bool TxBegin()
        {
            Console.WriteLine("[Client.TxBegin] Entering Client.TxBegin");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                /* 2. Chamar o metodo do servidor que dá inicio a transação */
                _currentTxInt = mainServer.StartTransaction();

                /* DEBUG PROPOSES */
                Console.WriteLine("[Client.TxBegin] txInt: {0}", _currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxBegin] Exception caught : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /*
         * This method attempts to commit the current transaction and returns a boolean value
         * indicating whether the operation succeded. This method may throw a TxException
         */

        public static bool TxCommit()
        {
            /* 1. Chamar o metodo do servidor que dá inicio ao commit da transação */
            Console.WriteLine("[Client.TxCommit] Entering Client.TxCommit");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                /* 2. Chamar o metodo do servidor que dá inicio ao commit da transação */
                mainServer.CommitTransaction(_currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxCommit] Exception caught : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /*
         * This method aborts the current transaction and returns a boolean value indicating
         * whether the operation succeeded. This method may throw a TxException
         */

        public static bool TxAbort()
        {
            /* 1. Chamar o metodo do servidor que aborta current transação*/
            Console.WriteLine("[Client.TxAbort] Entering Client.TxAbort");
            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal (Duvida: todas as transacoes vao primeiro ao main server certo) */
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                /* 2. Chamar o metodo do servidor que dá inicio ao commit da transação */
                mainServer.AbortTransaction(_currentTxInt);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.TxAbort] Exception caught : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /*
         * This method makes all nodes in the system dump to their output their current state
         */

        public static bool Status()
        {
            /* Se não se pode enviar a lista de Servers que o cliente já conhece à priori (devido ao Init()) a
             * melhor solução será pedir ao Main Server que por sua vez vai perguntar a todos os servidores
             * conhecidos qual o seu estado */

            Console.WriteLine("[Client.Status] Entering Status");

            try
            {
                /* 1. Tem de ser criada a ligação com o servidor principal */
                var mainServer = (IMainServer) Activator.GetObject(typeof (IMainServer), Config.RemoteMainserverUrl);

                /* 2. Temos que obter a list dos status dos servidores */
                var ex = mainServer.GetServerStatus();

                /* DEBUG PROPOSES */
                Console.WriteLine("[Servers.Status] {0}", ex);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Status] Exception : {0}", e.StackTrace);
                return false;
            }

            return true;
        }

        /*
         * This method makes the server at the URL stop responding to external calls except for
         * a Recover call
         */

        public static bool Fail(String url)
        {
            Console.WriteLine("[Client.Fail] Entering Fail");

            bool fail;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                var server = (IServer) Activator.GetObject(typeof (IServer), url);

                /* 2. Chamar metodo presente nele que congela ele proprio */
                fail = server.Fail();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Fail] Exception : {0}", e.StackTrace);
                return false;
            }

            return fail;
        }

        /*
         * This method makes the server at URL stop responding to external calls but it
         * maintains all calls for later reply, as if the communication to that server were
         * only delayed. A server in freeze mode responds immediately only to a Recover
         * call (see below), which triggers the execution of the backlog of operations
         * accumulated since the Freeze call
         */

        public static bool Freeze(String url)
        {
            Console.WriteLine("[Client.Freeze] Entering Freeze");

            bool freeze;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                var server = (IServer) Activator.GetObject(typeof (IServer), url);

                /* 2. Chamar metodo presente nele que congela ele proprio */
                freeze = server.Freeze();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Freeze] Exception : {0}", e.StackTrace);
                return false;
            }

            return freeze;
        }

        /*
         * This method creates a new shared object with the given uid. Returns null if the
         * object already exists.
         */

        public static bool Recover(String url)
        {
            Console.WriteLine("[Client.Recover] Entering Recover");

            bool recover;

            try
            {
                /* 1. Deverá ser criada uma connecção com o servidor indicado no "url" */
                var server = (IServer) Activator.GetObject(typeof (IServer), url);

                /* 2. Chamar metodo presente nele que renicia-o */
                recover = server.Recover();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.Recover] Exception : {0}", e.StackTrace);
                return false;
            }

            return recover;
        }

        /*
         * INTERACTION WITH SHARED DISTRIBUTED OBJECTS (PADINT)
         */

        /*
         * This method creates a new shared object with the given uid. Returns null if
         * the object already exists
         */

        public static PadInt CreatePadInt(int uid)
        {
            Console.WriteLine("[Client.CreatePadInt] Entering AccessPadInt");

            PadInt newPadInt = null;

            /* 2. Verificar se o tem e caso n tiver, cria-lo e retorna-o; caso ja exista retorna null */
            try
            {
                newPadInt = GetPadInt(uid);
                newPadInt.Read(); // If it reads the padint is already created
                Console.WriteLine("[Client.AccessPadInt] PadInt {0} already exits", uid);
            }
            catch (TxException)
            {
                Debug.Assert(newPadInt != null, "newPadInt != null");

                newPadInt.Write(0); // Initialize PadInt
                return newPadInt;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.CreatePadInt] Exception : {0}", e.StackTrace);
                return null;
            }

            return null;
        }

        /*
         * This method returns a reference to a shared object with the given uid. Returns
         * null if the object does not exist already
         */

        public static PadInt AccessPadInt(int uid)
        {
            Console.WriteLine("[Client.AccessPadInt] Entering AccessPadInt");

            PadInt accPadInt;

            /* 2. Verificar se o tem e caso o tiver, devolve-lo; caso n tiver retornar null */
            try
            {
                accPadInt = GetPadInt(uid);
                accPadInt.Read(); // If it doesn't read the padint doesn't exist
            }
            catch (TxException)
            {
                Console.WriteLine("[Client.AccessPadInt] PadInt {0} doesn't exits", uid);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client.AccessPadInt] Exception : {0}", e.StackTrace);
                return null;
            }
            return accPadInt;
        }

        private static PadInt GetPadInt(int uid)
        {
            var serverNum = uid%_serverList.Count;
            var serverId = _serverList.ToArray()[serverNum];
            var serverUrl = Config.GetServerUrl(serverId);

            var server = (IServer) Activator.GetObject(typeof (IServer), serverUrl);

            return new PadInt(_currentTxInt, uid, server);
        }
    }
}