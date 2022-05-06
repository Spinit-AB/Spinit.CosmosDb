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
            if (options != null && options.ThroughputProperties is { }  throughput && !ThroughputValidator.IsValidThroughput(throughput, out var message))
            {
                throw new ArgumentException(message);
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
        public async Task<ThroughputProperties> GetThroughputAsync()
        {
            return await _cosmosClient.GetDatabase(_database.Model.DatabaseId).ReadThroughputAsync(new RequestOptions()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the throughput (RU/s) for the database.
        /// </summary>
        /// <param name="throughputProperties">The new throughputProperties to set.</param>
        /// <returns></returns>
        public Task SetThroughputAsync(ThroughputProperties throughputProperties)
        {
            if (throughputProperties is { } && !ThroughputValidator.IsValidThroughput(throughputProperties, out var message))
            {
                throw new ArgumentException(message, nameof(throughputProperties));
            }

            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).ReplaceThroughputAsync(throughputProperties);
        }

        private async Task CreateIfNotExistsInternalAsync(CreateDbOptions options = null)
        {
            var databaseId = _database.Model.DatabaseId;
            var databaseThroughput = options != null && options.ThroughputType == ThroughputType.Database ? options.ThroughputProperties : (ThroughputProperties) null;
            var containerThroughput = options != null && options.ThroughputType == ThroughputType.Container ? options.ThroughputProperties : (ThroughputProperties) null;

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