using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADI_DSTM;

namespace Freeze
{
    class Program
    {
        static void Main(string[] args)
        {
            PadiDstm.Init();
            PadiDstm.Freeze("tcp://localhost:2001/Server");
            Console.ReadLine();
            PadiDstm.Recover("tcp://localhost:2001/Server");
        }
    }
}
