namespace Spinit.CosmosDb.Validation
{
    internal static class ThroughputValidator
    {
        internal static bool IsValidThroughput(int throughput) => throughput % 100 == 0 && throughput >= 400 && throughput <= 1000000;
    }
}
