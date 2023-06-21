using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;
using static Spinit.CosmosDb.Tests.Integration.FunctionalCollectionThroughputTests;

namespace Spinit.CosmosDb.Tests.Integration
{
    [TestCaseOrderer("Spinit.CosmosDb.Tests.Core.Order.TestCaseByAttributeOrderer", "Spinit.CosmosDb.Tests")]
    [Collection("DatabaseIntegrationTest")]
    public class FunctionalCollectionThroughputTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabase _database;

        public FunctionalCollectionThroughputTests(TestDatabaseFixture fixture)
        {
            _database = fixture.Database;
        }

        [Fact]
        [TestOrder]
        public async Task CreateDatabaseWithContainerThroughput()
        {
            await _database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(1000, ThroughputType.Container));
        }

        [Fact]
        [TestOrder]
        public async Task TestReadCollectionThroughput()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            Assert.Equal(1000, throughputProperties.Throughput);
        }

        [Fact]
        [TestOrder]
        public async Task TestSetDatabaseThroughput()
        {
            await _database.Todos.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(400));
        }

        [Fact]
        [TestOrder]
        public async Task TestGetDatabaseThroughputAfterSet()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            Assert.Equal(400, throughputProperties.Throughput);
        }


        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options) : base(options) { }

            public ICosmosDbCollection<TodoItem> Todos { get; set; }
        }

        public class TodoItem : ICosmosEntity
        {
            public string Id { get; set; }
        }

        public class TestDatabaseFixture : CosmosDatabaseFixture<TestDatabase>
        {
            public TestDatabaseFixture(DatabaseFixture databaseFixture) : base(databaseFixture)
            {
            }

            // Do not create database in this step
            public override Task InitializeAsync() => Task.CompletedTask;
        }
    }
}
