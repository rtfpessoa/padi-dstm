using System.Collections.Generic;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        int AddServer(string endpoint);

        void RemoveServer(int uid);

        Dictionary<int, string> ListServers();
    }
}