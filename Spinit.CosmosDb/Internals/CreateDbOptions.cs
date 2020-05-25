namespace Spinit.CosmosDb
{
    public class CreateDbOptions
    {
        public CreateDbOptions(int throughput, ThroughputType throughputType)
        {
            Throughput = throughput;
            ThroughputType = throughputType;
        }

        public int Throughput { get; }
        public ThroughputType ThroughputType { get; }
    }
}
