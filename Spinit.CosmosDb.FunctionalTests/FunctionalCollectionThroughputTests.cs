using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class FunctionalCollectionThroughputTests : IClassFixture<FunctionalCollectionThroughputTests.TestDatabase>
    {
        private readonly TestDatabase _database;

        public FunctionalCollectionThroughputTests(TestDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task CreateDatabaseWithContainerThroughput()
        {
            await _database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(1000, ThroughputType.Container));
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReadCollectionThroughput()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            Assert.Equal(1000, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestSetDatabaseThroughput()
        {
            await _database.Todos.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(400));
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestGetDatabaseThroughputAfterSet()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            Assert.Equal(400, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestDeleteDatabase()
        {
            await _database.Operations.DeleteAsync();
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
            }

            public ICosmosDbCollection<TodoItem> Todos { get; set; }
        }

        public class TodoItem : ICosmosEntity
        {
            public string Id { get; set; }
        }
    }
}
