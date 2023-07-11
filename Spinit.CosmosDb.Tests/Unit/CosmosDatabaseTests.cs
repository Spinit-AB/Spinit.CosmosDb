using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit
{
    public class CosmosDatabaseTests
    {
        [Fact]
        public void ShouldAutoCreateCollectionProperties()
        {
            var cosmosClient = Mock.Of<CosmosClient>();

            var database = new TestDatabase(cosmosClient);
            Assert.NotNull(database.TestEntities);
        }

        private class TestDatabase : CosmosDatabase
        {
            public TestDatabase(CosmosClient client) : base(client, new DatabaseOptions<TestDatabase>()) { }

            [CollectionId("TestEntities")]
            public ICosmosDbCollection<TestEntity>? TestEntities { get; set; }

            public class TestEntity : ICosmosEntity
            {
                public required string Id { get; set; }
                public string? Title { get; set; }
            }
        }
    }
}
