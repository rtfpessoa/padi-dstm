using CommonTypes;
using CommonTypes.NameRegistry;
using System.Collections.Generic;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        ServerInit AddServer();

        void RemoveServer(int uid);

        Dictionary<int, RegistryEntry> ListServers();
    }
}