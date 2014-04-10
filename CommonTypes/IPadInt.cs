using ServerLib.Transactions;

namespace CommonTypes
{
    public interface IPadInt
    {
        /* Reads the object in the context of the current transaction. Returns the value
         * of the object. This method may throw a TxException.
         */

        /// <exception cref="TxException"></exception>
        int Read();

        /* Writes the object in the context of the current transaction. This
         * method may throw a TxException.
         */

        /// <exception cref="TxException"></exception>
        void Write(int value);
    }
}