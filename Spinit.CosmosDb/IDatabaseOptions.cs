namespace Spinit.CosmosDb
{
    public interface IDatabaseOptions
    {
        string Endpoint { get; set; }
        string Key { get; set; }
        string PreferredLocation { get; set; }
    }
}
