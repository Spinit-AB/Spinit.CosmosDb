using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Spinit.CosmosDb
{
    internal class DatabaseOperations : IDatabaseOperations
    {
        private readonly CosmosDatabase _database;
        private readonly IDocumentClient _documentClient;

        internal DatabaseOperations(CosmosDatabase database, IDocumentClient documentClient)
        {
            _database = database;
            _documentClient = documentClient;
        }

        /// <summary>
        /// Creates the Cosmos database and defined collections if not exists.
        /// </summary>
        /// <returns></returns>
        public async Task CreateIfNotExistsAsync()
        {
            var databaseId = _database.GetDatabaseId();
            var database = new Database
            {
                Id = databaseId
            };
            await _documentClient.CreateDatabaseIfNotExistsAsync(database).ConfigureAwait(false);
            foreach (var collectionProperty in _database.GetCollectionProperties())
            {
                await _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseId),
                    new DocumentCollection
                    {
                        Id = _database.GetCollectionId(collectionProperty),
                        PartitionKey = new PartitionKeyDefinition { Paths = new Collection<string> { "/id" } },
                        IndexingPolicy = new IndexingPolicy
                        {
                            IndexingMode = IndexingMode.Consistent,
                            IncludedPaths = new Collection<IncludedPath>
                            {
                                new IncludedPath
                                {
                                    Path = "/*",
                                    Indexes = new Collection<Index>
                                    {
                                        new RangeIndex(DataType.Number, -1),
                                        new RangeIndex(DataType.String, -1)
                                    }
                                }
                            }
                        }
                    }
                    ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync()
        {
            var databaseId = _database.GetDatabaseId();
            var database = new Database
            {
                Id = databaseId
            };
            return _documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
        }
    }
}
