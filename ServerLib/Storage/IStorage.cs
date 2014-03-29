using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLib.Storage
{
    interface IStorage
    {
        int ReadPadInt(int transaction, int uuid);

        void WritePadInt(int transaction, int uuid, int value);

        Boolean PrepareTransaction(int transaction);

        Boolean CommitTransaction(int transaction);

        Boolean AbortTransaction(int transaction);
    }
}
