using Xunit;

namespace Spinit.CosmosDb.Tests.Core
{
    public class CosmosDatabaseFixture<TDatabase> : IAsyncLifetime where TDatabase : CosmosDatabase
    {
        public CosmosDatabaseFixture(DatabaseFixture databaseFixture)
        {
            DatabaseFixture = databaseFixture;
            Database = databaseFixture.CreateDatabase<TDatabase>();
        }

        public DatabaseFixture DatabaseFixture { get; }
        public TDatabase Database { get; }

        /// <summary>
        /// Creates db (if not exists) and all containers used by the <see cref="TDatabase"/>
        /// </summary>
        /// <returns></returns>
        public virtual Task InitializeAsync() => Database.Operations.CreateIfNotExistsAsync();

        /// <summary>
        /// Deletes all containers used by the <see cref="TDatabase"/>, but not the database itself
        /// </summary>
        /// <returns></returns>
        public virtual async Task DisposeAsync()
        {
            var collectionIds = Database.Model.CollectionModels.Select(x => x.CollectionId);
            await DatabaseFixture.Clear(collectionIds);
        }
    }
}
