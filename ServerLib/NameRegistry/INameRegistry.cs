using System.Collections.Generic;
using CommonTypes;
using CommonTypes.NameRegistry;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        ServerInit AddServer();

        Dictionary<int, RegistryEntry> ListServers();
    }
}