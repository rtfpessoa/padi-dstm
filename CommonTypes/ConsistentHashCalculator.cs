namespace CommonTypes
{
    public class ConsistentHashCalculator
    {
        public static int GetServerIdForPadInt(int serverCount, int padIntId)
        {
            return padIntId%serverCount;
        }

        public static bool IsMyPadInt(int serverCount, int padIntId, int serverId)
        {
            return (padIntId%serverCount) == serverId;
        }
    }
}