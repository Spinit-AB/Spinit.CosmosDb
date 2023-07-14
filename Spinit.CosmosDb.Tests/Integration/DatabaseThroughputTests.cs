using Microsoft.Azure.Cosmos;
using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    [TestCaseOrderer("Spinit.CosmosDb.Tests.Core.Order.TestCaseByAttributeOrderer", "Spinit.CosmosDb.Tests")]
    [Collection("DatabaseIntegrationTest")]
    public class DatabaseThroughputTests : IClassFixture<DatabaseThroughputTests.TestDatabaseFixture>
    {
        private readonly TestDatabase _database;

        public DatabaseThroughputTests(TestDatabaseFixture fixture)
        {
            _database = fixture.Database;
        }

        [Fact]
        [TestOrder]
        public async Task CreateDatabaseWithDatabaseThroughput()
        {
            await _database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(1000, ThroughputType.Database));
        }

        [Fact]
        [TestOrder]
        public async Task TestReadDatabaseThroughput()
        {
            var throughputProperties = await _database.Operations.GetThroughputAsync();
            throughputProperties.Throughput.ShouldBe(1000);
        }

        [Fact]
        [TestOrder]
        public async Task TestSetDatabaseThroughput()
        {
            await _database.Operations.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(400));
        }

        [Fact]
        [TestOrder]
        public async Task TestGetDatabaseThroughputAfterSet()
        {
            var throughputProperties = await _database.Operations.GetThroughputAsync();
            throughputProperties.Throughput.ShouldBe(400);
        }


        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options) : base(options) { }
        }

        public class TestDatabaseFixture : IAsyncLifetime
        {
            public TestDatabaseFixture()
            {
                Adapter = new DatabaseAdapter(databaseName: "Database_Throughput_Tests");
                Database = Adapter.CreateCosmosDbDatabase<TestDatabase>();
            }

            public TestDatabase Database { get; }
            private DatabaseAdapter Adapter { get; }

            // Do not create database in this step
            public Task InitializeAsync() => Task.CompletedTask;

            public Task DisposeAsync() => Adapter.TeardownDatabase();
        }
    }
}
