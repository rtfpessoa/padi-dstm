using System.Collections.Generic;

namespace CommonTypes.Storage
{
    public interface IStorage
    {
        int ReadValue(int key);

        void WriteValue(int key, int value);

        bool HasValue(int key);

        List<KeyValuePair<int, int>> GetValues();
    }
}