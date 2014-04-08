using CommonTypes;
using ServerLib.Transactions;
using System;
using System.IO;
using System.Collections.Generic;


using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MainServer
{
    internal class MainServer : Coordinator, IMainServer
    {
        /* Give the Server List */
        public string[] getServerList() {

            Console.WriteLine("[GetServerList] Entering GetServerList method");

            /* Create a string List because we don't know how much servers exist */
            List<string> serversList = new List<string>();

            /* Try to red from file 'ServerList' */
            try
            {
                using (StreamReader sr = new StreamReader("..\\..\\ServerList.txt"))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null) {
                        serversList.Add(line);
                        Console.WriteLine("[GetServerList] Read line {0} from ServerList.txt", line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("[GetServerList] Exiting GetServerList method");

            /* Return a string array */
            return serversList.ToArray();
        }

        /* Give the Server List */
        public string[] getServerStatus()
        {
            Console.WriteLine("[GetServerStatus] Entering GetServerList method");

            /* Create a string List because we don't know how much servers exist */
            List<string> serversStatusList = new List<string>();

            /* Try to red from file 'ServerList' */
            try
            {
                using (StreamReader sr = new StreamReader("..\\..\\ServerList.txt"))
                {
                    String serverURL;
                    while ((serverURL = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("[GetServerStatus] Read line {0} from ServerList.txt", serverURL);
                        try
                        {
                            IServer server = (IServer)Activator.GetObject(typeof(IServer), serverURL);

                            /* TALVEZ AQUI SEJA PRECISO ALGO MAIS DO MAIN SERVER */

                            /* 2. Temos que obter a list de servidores do sistema dada pelo MS */
                            serversStatusList.Add(server.Status());

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[GetServerStatus] Cannot contact server {0}", serverURL);
                            Console.WriteLine("[GetServerStatus] {0}", e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[GetServerStatus] The file could not be read:");
                Console.WriteLine("[GetServerStatus] {0}", e.Message);
            }

            Console.WriteLine("[GetServerStatus] Exiting GetServerList method");

            /* Return a string array */
            return serversStatusList.ToArray();
        }
    }
}