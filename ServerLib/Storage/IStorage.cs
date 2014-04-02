using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLib.Storage
{
    public interface IStorage
    {
        int ReadValue(int key);

        void WriteValue(int key, int value);
    }
}