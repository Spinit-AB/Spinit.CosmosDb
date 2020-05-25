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
        public Task CreateIfNotExistsAsync(CreateDbOptions options = null)
        {
            if (options != null && !ThroughputValidator.IsValidThroughput(options.Throughput))
            {
                throw new ArgumentException("The provided throughput is not valid. Must be between 400 and 1000000 and in increments of 100.");
            }

            return CreateIfNotExistsInternalAsync(options);
        }

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync()
        {
            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).DeleteAsync();
        }

        /// <summary>
        /// Gets the throughput (RU/s) set for the collection.
        /// </summary>
        /// <returns></returns>
        public Task<int?> GetThroughputAsync()
        {
            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).ReadThroughputAsync();
        }

        /// <summary>
        /// Sets the throughput (RU/s) for the database.
        /// </summary>
        /// <param name="throughput">The new throughput to set. Must be between 400 and 1000000 in increments of 100.</param>
        /// <returns></returns>
        public Task SetThroughputAsync(int throughput)
        {
            if (!ThroughputValidator.IsValidThroughput(throughput))
            {
                throw new ArgumentException("The provided throughput is not valid. Must be between 400 and 1000000 and in increments of 100.", nameof(throughput));
            }

            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).ReplaceThroughputAsync(throughput);
        }

        private async Task CreateIfNotExistsInternalAsync(CreateDbOptions options = null)
        {
            var databaseId = _database.Model.DatabaseId;
            var databaseThroughput = options != null && options.ThroughputType == ThroughputType.Database ? options.Throughput : (int?)null;
            var containerThroughput = options != null && options.ThroughputType == ThroughputType.Container ? options.Throughput : (int?)null;

            await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId, databaseThroughput).ConfigureAwait(false);
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
    }
}