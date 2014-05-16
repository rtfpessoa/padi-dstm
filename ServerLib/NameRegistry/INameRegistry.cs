using CommonTypes;
using CommonTypes.NameRegistry;
using System.Collections.Generic;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        ServerInit AddServer();

        Dictionary<int, RegistryEntry> ListServers();
    }
}