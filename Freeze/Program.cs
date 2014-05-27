using System;
using PADI_DSTM;

namespace Freeze
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            PadiDstm.Init();
            PadiDstm.Freeze("tcp://localhost:2001/Server");
            Console.ReadLine();
            PadiDstm.Recover("tcp://localhost:2001/Server");
        }
    }
}