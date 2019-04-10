namespace Spinit.CosmosDb
{
    public class DatabaseOptions<TDatabase> : IDatabaseOptions
        where TDatabase : CosmosDatabase
    {
        public string Endpoint { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// Database id/name
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Optional preferred location
        /// </summary>
        public string PreferredLocation { get; set; }
    }
}
