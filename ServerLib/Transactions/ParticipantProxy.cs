﻿using System;
using System.Collections.Generic;

namespace ServerLib.Transactions
{
    internal class ParticipantProxy
    {
        private static class ParticipantProxyCache
        {
            private static Dictionary<string, IParticipant> participants = new Dictionary<string, IParticipant>();

            public static IParticipant GetParticipant(string endpoint)
            {
                IParticipant participant;

                if (!participants.TryGetValue(endpoint, out participant))
                {
                    participant = (IParticipant)Activator.GetObject(typeof(IParticipant), endpoint);
                }

                return participant;
            }
        }

        public string endpoint;
        public bool readyToCommit = false;

        public ParticipantProxy(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public IParticipant GetProxy()
        {
            return ParticipantProxyCache.GetParticipant(endpoint);
        }
    }
}