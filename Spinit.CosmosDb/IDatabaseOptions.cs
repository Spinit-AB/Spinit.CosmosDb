namespace Spinit.CosmosDb
{
    public interface IDatabaseOptions
    {
        string Endpoint { get; set; }

        string Key { get; set; }

        /// <summary>
        /// Database id/name
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// Optional preferred location
        /// </summary>
        string PreferredLocation { get; set; }
    }
}
