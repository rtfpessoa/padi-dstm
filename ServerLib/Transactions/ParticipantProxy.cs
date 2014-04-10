using System;
using System.Collections.Generic;

namespace ServerLib.Transactions
{
    internal class ParticipantProxy
    {
        public string Endpoint;
        public bool ReadyToCommit = false;

        public ParticipantProxy(string endpoint)
        {
            Endpoint = endpoint;
        }

        public IParticipant GetProxy()
        {
            return ParticipantProxyCache.GetParticipant(Endpoint);
        }

        private static class ParticipantProxyCache
        {
            private static readonly Dictionary<string, IParticipant> Participants =
                new Dictionary<string, IParticipant>();

            public static IParticipant GetParticipant(string endpoint)
            {
                IParticipant participant;

                if (!Participants.TryGetValue(endpoint, out participant))
                {
                    participant = (IParticipant) Activator.GetObject(typeof (IParticipant), endpoint);
                }

                return participant;
            }
        }
    }
}