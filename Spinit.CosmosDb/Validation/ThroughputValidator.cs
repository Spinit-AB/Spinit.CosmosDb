using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb.Validation
{
    internal static class ThroughputValidator
    {
        internal static bool IsValidManualThroughput(int throughput) => throughput % 100 == 0 && throughput >= 400 && throughput <= 1000000;
        internal static bool IsValidAutoscaleThroughput(int throughput) => throughput % 1000 == 0 && throughput >= 4000 && throughput <= 1000000;

        internal static bool IsValidThroughput(ThroughputProperties throughputProperties, out string message)
        {
            if (throughputProperties.Throughput is int manualThroughput && !IsValidManualThroughput(manualThroughput))
            {
                message = "The provided manual throughput is not valid. Must be between 400 and 1000000 and in increments of 100.";
                return false;
            }
            else if(throughputProperties.AutoscaleMaxThroughput is int autoscaleMaxThroughput && !IsValidAutoscaleThroughput(autoscaleMaxThroughput))
            {
                message = "The provided autoscale throughput is not valid. Must be between 4000 and 1000000 and in increments of 1000.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
