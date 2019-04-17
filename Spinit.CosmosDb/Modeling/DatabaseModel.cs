using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    public sealed class DatabaseModel
    {
        public DatabaseModel()
        {
            CollectionModels = new List<CollectionModel>();
        }

        public string DatabaseId { get; internal set; }

        public IEnumerable<CollectionModel> CollectionModels { get; internal set; }
    }
}
