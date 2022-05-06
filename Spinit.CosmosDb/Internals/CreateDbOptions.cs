using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb
{
    public class CreateDbOptions
    {
        /// <summary>
        /// Options for when creating a new database.
        /// </summary>
        /// <param name="throughput">The throughput to set. Must be between 400 and 1000000 and in increments of 100.</param>
        /// <param name="throughputType">Where to set the default throughput. Sets the throughput for all containers if Container is selected.</param>
        public CreateDbOptions(int throughput, ThroughputType throughputType)
        {
            ThroughputProperties = ThroughputProperties.CreateManualThroughput(throughput);
            ThroughputType = throughputType;
        }

        /// <summary>
        /// Options for when creating a new database.
        /// </summary>
        /// <param name="throughputProperties">The throughput to set.</param>
        /// <param name="throughputType">Where to set the default throughput. Sets the throughput for all containers if Container is selected.</param>
        public CreateDbOptions(ThroughputProperties throughputProperties, ThroughputType throughputType)
        {
            ThroughputProperties = throughputProperties;
            ThroughputType = throughputType;
        }

        public ThroughputProperties ThroughputProperties { get; }
        public ThroughputType ThroughputType { get; }
    }
}
