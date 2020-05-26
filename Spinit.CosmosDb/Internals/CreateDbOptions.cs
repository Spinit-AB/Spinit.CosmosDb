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
            Throughput = throughput;
            ThroughputType = throughputType;
        }

        public int Throughput { get; }
        public ThroughputType ThroughputType { get; }
    }
}
