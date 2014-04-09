using System.Collections.Generic;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        int AddServer();

        void RemoveServer(int uid);

        List<int> ListServers();
    }
}