namespace Spinit.CosmosDb
{
    public enum AnalyzeContext
    {
        /// <summary>
        /// Analyze tokens for an entity used when indexing/upserting
        /// </summary>
        Entity = 1,

        /// <summary>
        /// Analyze tokens for a search query
        /// </summary>
        Query = 2
    }
}
