using Microsoft.Azure.Documents;
using Moq;
using Xunit;

namespace Spinit.CosmosDb.UnitTests
{
    public class CosmosDatabaseTests
    {
        [Fact]
        public void CreateConnectionPolicyShouldSetPreferredLocation()
        {
            var config = new DatabaseOptions<CosmosDatabase>
            {
                PreferredLocation = "West Europe"
            };
            var connectionPolicy = CosmosDatabase.CreateConnectionPolicy(config);
            Assert.Contains(connectionPolicy.PreferredLocations, x => x == config.PreferredLocation);
        }

        [Fact]
        public void ShouldAutoCreateCollectionProperties()
        {
            var client = Mock.Of<IDocumentClient>();
            var database = new TestDatabase(client);
            Assert.NotNull(database.TestEntities);
        }

        private class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDocumentClient client)
                : base(client, new DatabaseOptions<TestDatabase>())
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
