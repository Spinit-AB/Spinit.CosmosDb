using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb.Tests.Core
{
    public class DatabaseAdapter : IDisposable
    {
        public DatabaseAdapter(string? connectionString = null, string? databaseName = null)
        {
            DatabaseName = databaseName ?? $"TestDatabase_{DateTime.Now:yyyy-MM-dd_HH-mm}";
            ConnectionString = connectionString ?? "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            Client = new CosmosClient(ConnectionString);
        }

        public string DatabaseName { get; }

        public string ConnectionString { get; }

        protected CosmosClient Client { get; }

        public async Task SetupDatabase()
        {
            await Client.CreateDatabaseIfNotExistsAsync(DatabaseName);
        }

        public TDatabase CreateDatabase<TDatabase>(Action<DatabaseOptionsBuilder<TDatabase>>? options = null) where TDatabase : CosmosDatabase
        {
            var optionsBuilder = new DatabaseOptionsBuilder<TDatabase>();
            optionsBuilder.UseConnectionString(ConnectionString);
            optionsBuilder.Options.DatabaseId = DatabaseName;
            options?.Invoke(optionsBuilder);

            if (Activator.CreateInstance(typeof(TDatabase), optionsBuilder.Options) is TDatabase database)
            {
                return database;
            }

            throw new Exception($"Could not create database of type {typeof(TDatabase)}");
        }

        public async Task Clear()
        {
            var database = Client.GetDatabase(DatabaseName);
            var iterator = database.GetContainerQueryIterator<ContainerProperties>("select * from c");
            while(iterator.HasMoreResults)
            {
                var containerProps = await iterator.ReadNextAsync();
                foreach (var props in containerProps)
                {
                    var container = database.GetContainer(props.Id);
                    await container.DeleteContainerAsync();
                }
            }
        }

        public async Task Clear(IEnumerable<string> collectionIds)
        {
            var database = Client.GetDatabase(DatabaseName);
            foreach (var collectionId in collectionIds)
            {
                var container = database.GetContainer(collectionId);
                await container.DeleteContainerAsync();
            }
        }

        public async Task TeardownDatabase()
        {
            var database = Client.GetDatabase(DatabaseName);
            await database.DeleteAsync();
        }

        public void Dispose() => Client.Dispose();
    }
}