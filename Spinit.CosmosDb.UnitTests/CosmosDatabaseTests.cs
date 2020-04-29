using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;

namespace Spinit.CosmosDb.UnitTests
{
    public class CosmosDatabaseTests
    {
        [Fact]
        public void ShouldAutoCreateCollectionProperties()
        {
            var cosmosClient = Mock.Of<CosmosClient>();
            var documentClient = Mock.Of<Microsoft.Azure.Documents.Client.DocumentClient>();

            var database = new TestDatabase(documentClient, cosmosClient);
            Assert.NotNull(database.TestEntities);
        }

        private class TestDatabase : CosmosDatabase
        {
            public TestDatabase(Microsoft.Azure.Documents.Client.DocumentClient documentClient, CosmosClient client)
                : base(documentClient, client, new DatabaseOptions<TestDatabase>())
            { }

            [CollectionId("TestEntities")]
            public ICosmosDbCollection<TestEntity> TestEntities { get; set; }

            public class TestEntity : ICosmosEntity
            {
                public string Id { get; set; }
                public string Title { get; set; }
            }
        }
    }
}
