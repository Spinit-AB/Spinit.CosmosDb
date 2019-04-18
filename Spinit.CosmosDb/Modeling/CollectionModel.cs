namespace Spinit.CosmosDb
{
    public sealed class CollectionModel
    {
        public string DatabaseId { get; internal set; }

        public string CollectionId { get; internal set; }

        public Analyzer Analyzer { get; internal set; }
    }
}
