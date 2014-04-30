using CommonTypes;

namespace PADI_DSTM
{
    public class PadInt
    {
        private readonly int _txid;
        private readonly int _uid;

        private IServer _server;
        private int _version;

        public PadInt(int txid, int uid, IServer server, int version)
        {
            _txid = txid;
            _uid = uid;
            _server = server;
            _version = version;
        }

        public int Read()
        {
            int value;

            try
            {
                value = _server.ReadValue(_version, _txid, _uid);
            }
            catch (WrongVersionException)
            {
                PadiDstm.UpdateServers();
                PadInt newPadInt = PadiDstm.GetPadInt(_uid);
                _server = newPadInt._server;
                _version = newPadInt._version;

                value = newPadInt.Read();
            }

            return value;
        }

        public void Write(int value)
        {
            try
            {
                _server.WriteValue(_version, _txid, _uid, value);
            }
            catch (WrongVersionException)
            {
                PadiDstm.UpdateServers();
                PadInt newPadInt = PadiDstm.GetPadInt(_uid);
                _server = newPadInt._server;
                _version = newPadInt._version;

                newPadInt.Write(value);
            }
        }
    }
}