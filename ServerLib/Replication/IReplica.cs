namespace ServerLib.Replication
{
    public interface IReplica
    {
        void ReadThrough(int version, int txid, int key);

        void WriteThrough(int version, int txid, int key, int value);
    }
}