﻿using System.Collections.Generic;
using CommonTypes;

namespace ServerLib.NameRegistry
{
    public interface INameRegistry
    {
        ServerInit AddServer();

        void RemoveServer(int uid);

        List<int> ListServers();
    }
}