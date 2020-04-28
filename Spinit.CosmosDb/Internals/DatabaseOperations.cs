using Microsoft.Azure.Cosmos;
using System.Collections.ObjectModel;
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
        public async Task CreateIfNotExistsAsync()
        {
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
                    .WithDefaultTimeToLive(30)
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);



                //await _cosmosClient.CreateDocumentCollectionIfNotExistsAsync(
                //    UriFactory.CreateDatabaseUri(databaseId),
                //    new DocumentCollection
                //    {
                //        Id = collectionModel.CollectionId,
                //        PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { "/id" } },
                //        IndexingPolicy = new IndexingPolicy
                //        {
                //            IndexingMode = IndexingMode.Consistent,
                //            Automatic = true,
                //            IncludedPaths = new Collection<IncludedPath>
                //            {
                //                new IncludedPath
                //                {
                //                    Path = "/*",
                //                    Indexes = new Collection<Index>
                //                    {
                //                        new RangeIndex(DataType.Number, -1),
                //                        new RangeIndex(DataType.String, -1)
                //                    }
                //                },
                //                new IncludedPath
                //                {
                //                    Path = "/_all/*",
                //                    Indexes = new Collection<Index>
                //                    {
                //                        new HashIndex(DataType.Number, -1),
                //                        new HashIndex(DataType.String, -1)
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync()
        {
            //var databaseId = _database.Model.DatabaseId;
            //var database = new Database
            //{
            //    Id = databaseId
            //};

            //return _cosmosClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));

            return _cosmosClient.GetDatabase(_database.Model.DatabaseId).DeleteAsync();
        }
    }
}
