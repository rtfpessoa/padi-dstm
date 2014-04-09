using CommonTypes;
using System;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IServer server = new Server();

            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
        }
    }
}