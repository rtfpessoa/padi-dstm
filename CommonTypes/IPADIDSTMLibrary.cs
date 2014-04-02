namespace CommonTypes
{
    public interface IPadiDstmLibrary
    {
        /**/

        bool Init();

        /**/

        /// <exception cref="TxException"></exception>
        bool TxBegin();

        /* Attempts to commit the current transaction and returns
         * a boolean value indicating whether the operation succeded. This method may throw a
         * TxException.
         */

        /// <exception cref="TxException"></exception>
        bool TxCommit();

        /* Aborts the current transaction and returns a boolean value
         * indicating whether the operation succeeded. This method may throw a TxException.
         */

        /// <exception cref="TxException"></exception>
        bool TxAbort();

        /* All nodes in the system dump to their output their
        * current state.
        */

        bool Status();

        /* Makes the server at the URL stop responding to
         * external calls except for a Recover call (see below).
         */

        bool Fail(string URL);

        /* Makes the server at URL stop responding to external
         * calls but it maintains all calls for later reply, as if the communication to that server were
         * only delayed. A server in freeze mode responds immediately only to a Recover call (see
         * below), which triggers the execution of the backlog of operations accumulated since the
         * Freeze call.
         */

        bool Freeze(string URL);

        /* Makes the server at URL recover from a previous
         * Fail or Freeze call.         */

        bool Recover(string URL);

        /* Creates a new shared object with the given
         * uid. Returns null if the object already exists.
         */

        IPadInt CreatePadInt(int uid);

        /* Returns a reference to a shared object with
         * the given uid. Returns null if the object does not exist already
         */

        IPadInt AccessPadInt(int uid);
    }
}