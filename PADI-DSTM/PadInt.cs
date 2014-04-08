using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace PADI_DSTM
{
    public class PadInt : IPadInt
    {
        private readonly int uid;
        private readonly IServer server;

        public PadInt(int uid, IServer server)
        {
            this.uid = uid;
            this.server = server;
        }

        public int Read()
        {
            throw new NotImplementedException();
        }

        public void Write(int value)
        {
            throw new NotImplementedException();
        }
    }
}
