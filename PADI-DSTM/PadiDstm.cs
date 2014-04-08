using CommonTypes;
using System;

namespace PADI_DSTM
{
    public static class PadiDstm
    {
        private static IMainServer mainServer;

        public static bool Init()
        {
            try
            {
                mainServer = (IMainServer)Activator.GetObject(typeof(IMainServer), Config.REMOTE_MAINSERVER_URL);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /**/

        /// <exception cref="TxException"></exception>
        public static bool TxBegin()
        {
            throw new NotImplementedException();
        }

        /* Attempts to commit the current transaction and returns
         * a boolean value indicating whether the operation succeded. This method may throw a
         * TxException.
         */

        /// <exception cref="TxException"></exception>
        public static bool TxCommit()
        {
            throw new NotImplementedException();
        }

        /* Aborts the current transaction and returns a boolean value
         * indicating whether the operation succeeded. This method may throw a TxException.
         */

        /// <exception cref="TxException"></exception>
        public static bool TxAbort()
        {
            throw new NotImplementedException();
        }

        /* All nodes in the system dump to their output their
        * current state.
        */

        public static bool Status()
        {
            throw new NotImplementedException();
        }

        /* Makes the server at the URL stop responding to
         * external calls except for a Recover call (see below).
         */

        public static bool Fail(string URL)
        {
            throw new NotImplementedException();
        }

        /* Makes the server at URL stop responding to external
         * calls but it maintains all calls for later reply, as if the communication to that server were
         * only delayed. A server in freeze mode responds immediately only to a Recover call (see
         * below), which triggers the execution of the backlog of operations accumulated since the
         * Freeze call.
         */

        public static bool Freeze(string URL)
        {
            throw new NotImplementedException();
        }

        /* Makes the server at URL recover from a previous
         * Fail or Freeze call.
         */

        public static bool Recover(string URL)
        {
            throw new NotImplementedException();
        }

        /* Creates a new shared object with the given
         * uid. Returns null if the object already exists.
         */

        public static PadInt CreatePadInt(int uid)
        {
            throw new NotImplementedException();
        }

        /* Returns a reference to a shared object with
         * the given uid. Returns null if the object does not exist already
         */

        public static PadInt AccessPadInt(int uid)
        {
            throw new NotImplementedException();
        }
    }
}