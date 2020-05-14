using Microsoft.Azure.Cosmos;
using Spinit.CosmosDb.Validation;
using System;
using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    internal class DatabaseOperations : IDatabaseOperations
    {
        private readonly CosmosDatabase _database;
        private readonly CosmosClient _cosmosClient;

        internal DatabaseOperations(CosmosDatabase database, CosmosClient cosmosClient)
        {
            _database = database;
            _cosmosClient = cosmosClient;
        }

        /// <summary>
        /// Creates the Cosmos database and defined collections if not exists.
        /// </summary>
        /// <returns></returns>
        public async Task CreateIfNotExistsAsync(int containerThroughput = 400)
        {
            if (!ThroughputValidator.IsValidThroughput(containerThroughput))
            {
                throw new ArgumentException("The provided throughput is not valid. Must be between 400 and 1000000 and in increments of 100.", nameof(containerThroughput));
            }

            var databaseId = _database.Model.DatabaseId;

            await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).ConfigureAwait(false);
            foreach (var collectionModel in _database.Model.CollectionModels)
            {
                await _cosmosClient.GetDatabase(databaseId)
                    .DefineContainer(name: collectionModel.CollectionId, partitionKeyPath: "/id")
                        .WithIndexingPolicy()
                            .WithIncludedPaths()
                                .Path("/*")
                                .Path("/_all/*")
                                .Attach()
                        .WithIndexingMode(IndexingMode.Consistent)
                            .Attach()
                    .CreateIfNotExistsAsync(containerThroughput)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync()
        {
            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).DeleteAsync();
        }
    }
}
