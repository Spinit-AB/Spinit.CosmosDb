namespace Spinit.CosmosDb
{
    public class DatabaseOptions<TDatabase> : IDatabaseOptions
        where TDatabase : CosmosDatabase
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string PreferredLocation { get; set; }
    }
}
