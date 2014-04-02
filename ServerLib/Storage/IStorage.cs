namespace ServerLib.Storage
{
    public interface IStorage
    {
        int ReadValue(int key);

        void WriteValue(int key, int value);
    }
}