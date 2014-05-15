using CommonTypes;

namespace ServerLib.Transactions
{
    public interface IParticipant
    {
        /// <exception cref="TxException"></exception>
        int ReadValue(int txid, int key);

        /// <exception cref="TxException"></exception>
        void WriteValue(int txid, int key, int value);

        /// <exception cref="TxException"></exception>
        void PrepareTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void CommitTransaction(int txid);

        /// <exception cref="TxException"></exception>
        void AbortTransaction(int txid);

        ParticipantStatus GetStatus();

        void SetStatus(ParticipantStatus status);

        void DumpState();
    }
}