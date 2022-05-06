using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb.Validation
{
    internal static class ThroughputValidator
    {
        internal static bool IsValidManualThroughput(int throughput) => throughput % 100 == 0 && throughput >= 400 && throughput <= 1_000_000;
        internal static bool IsValidAutoscaleThroughput(int throughput) => throughput % 1000 == 0 && throughput >= 1000 && throughput <= 1_000_000;

        internal static bool IsValidThroughput(ThroughputProperties throughputProperties, out string message)
        {
            if (throughputProperties.Throughput is int manualThroughput && !IsValidManualThroughput(manualThroughput))
            {
                message = "The provided manual throughput is not valid. Must be between 400 and 1 000 000 and in increments of 100.";
                return false;
            }
            else if(throughputProperties.AutoscaleMaxThroughput is int autoscaleMaxThroughput && !IsValidAutoscaleThroughput(autoscaleMaxThroughput))
            {
                message = "The provided autoscale throughput is not valid. Must be between 1 000 and 1 000 000 and in increments of 1 000.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
