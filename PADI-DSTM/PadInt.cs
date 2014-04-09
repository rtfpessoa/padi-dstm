using CommonTypes;

namespace PADI_DSTM
{
    public class PadInt : IPadInt
    {
        private readonly IServer server;
        private readonly int txid;
        private readonly int uid;

        public PadInt(int txid, int uid, IServer server)
        {
            this.txid = txid;
            this.uid = uid;
            this.server = server;
        }

        public int Read()
        {
            return server.ReadValue(txid, uid);
        }

        public void Write(int value)
        {
            server.WriteValue(txid, uid, value);
        }
    }
}