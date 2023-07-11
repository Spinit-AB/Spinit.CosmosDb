using Microsoft.Azure.Cosmos;
using Xunit;

namespace Spinit.CosmosDb.Tests.Core
{
    [Collection("DatabaseIntegrationTest")]
    public abstract class DatabaseTestBase<TDatabase> : IAsyncLifetime, IClassFixture<CosmosDatabaseFixture<TDatabase>>
        where TDatabase : CosmosDatabase
    {
        public DatabaseTestBase(CosmosDatabaseFixture<TDatabase> cosmosDatabaseFixture)
        {
            DatabaseFixture = cosmosDatabaseFixture.DatabaseFixture;
            Database = cosmosDatabaseFixture.Database;
        }

        public DatabaseFixture DatabaseFixture { get; }
        public TDatabase Database { get; }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual async Task DisposeAsync()
        {
            // Clear all items in all containers of this database
            foreach (var collectionModel in Database.Model.CollectionModels)
            {
                var container = Database.CosmosClient.GetContainer(collectionModel.DatabaseId, collectionModel.CollectionId);
                var iterator = container.GetItemQueryIterator<CosmosDbEntity>("select * from c");
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        await container.DeleteItemAsync<object>(item.Id, new PartitionKey(item.Id));
                    }
                }
            }
        }

        private class CosmosDbEntity : ICosmosEntity
        {
            public required string Id { get; set; }
        }
    }
}
