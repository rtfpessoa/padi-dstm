using CommonTypes;

namespace PADI_DSTM
{
    public class PadInt : IPadInt
    {
        private readonly IServer _server;
        private readonly int _txid;
        private readonly int _uid;

        public PadInt(int txid, int uid, IServer server)
        {
            _txid = txid;
            _uid = uid;
            _server = server;
        }

        public int Read()
        {
            return _server.ReadValue(_txid, _uid);
        }

        public void Write(int value)
        {
            _server.WriteValue(_txid, _uid, value);
        }
    }
}