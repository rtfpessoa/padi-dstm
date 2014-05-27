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

        public IPartitipantProxy GetProxy()
        {
            return ParticipantProxyCache.GetParticipant(Endpoint);
        }

        private static class ParticipantProxyCache
        {
            private static readonly Dictionary<string, IPartitipantProxy> Participants =
                new Dictionary<string, IPartitipantProxy>();

            public static IPartitipantProxy GetParticipant(string endpoint)
            {
                IPartitipantProxy participant;

                if (!Participants.TryGetValue(endpoint, out participant))
                {
                    participant = (IPartitipantProxy) Activator.GetObject(typeof (IPartitipantProxy), endpoint);
                }

                return participant;
            }
        }
    }
}