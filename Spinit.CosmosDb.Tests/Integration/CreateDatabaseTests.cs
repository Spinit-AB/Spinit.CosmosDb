using Microsoft.Azure.Cosmos;
using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    public class CreateDatabaseTests
    {
        public CreateDatabaseTests() => Adapter = new DatabaseAdapter(databaseName: "Create_Database_Tests");

        public DatabaseAdapter Adapter { get; }

        [Fact]
        [TestOrder]
        public async Task TestCreateDatabase()
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync();
            await database.Operations.DeleteAsync();
        }

        [Fact]
        [TestOrder]
        public async Task TestCreateDatabaseWithoutDefaultThroughputSet()
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync();
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            throughputProperties.Throughput.ShouldBe(400);
        }

        [Theory]
        [TestOrder]
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithtDefaultContainerThroughputSet(int defaultThroughput, int expected)
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            throughputProperties.Throughput.ShouldBe(expected);
        }

        [Theory]
        [TestOrder]
        [InlineData(399)]
        [InlineData(10001)]
        [InlineData(550)]
        [InlineData(999)]
        public async Task TestCreateDatabaseWithInvalidThroughputSet(int defaultThroughput)
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();
            await Should.ThrowAsync<ArgumentException>(async () => await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Container)));
        }

        [Theory]
        [TestOrder]
        [InlineData(400)]
        [InlineData(3999)]
        [InlineData(4500)]
        [InlineData(1001000)]
        public async Task TestCreateDatabaseWithInvalidAutoscaleThroughputSet(int maxThroughput)
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await Should.ThrowAsync<ArgumentException>(async () => await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateAutoscaleThroughput(maxThroughput), ThroughputType.Container)));
        }

        [Theory]
        [TestOrder]
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithDefaultDatabaseThroughputSet(int defaultThroughput, int expected)
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Database));
            var throughputProperties = await database.Operations.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            throughputProperties.Throughput.ShouldBe(expected);
        }

        [Fact]
        [TestOrder]
        public async Task TestCreateDatabaseWithCustomContainerThroughputSet()
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateManualThroughput(500), ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            throughputProperties.Throughput.ShouldBe(500);
        }

        [Theory]
        [TestOrder]
        [InlineData(1000)]
        [InlineData(4000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public async Task TestCreateDatabaseWithAutoscaleContainerThroughputSet(int maxThroughput)
        {
            var database = Adapter.CreateCosmosDbDatabase<DummyDatabase>();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateAutoscaleThroughput(maxThroughput), ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            var minThroughput = maxThroughput / 10;
            throughputProperties.Throughput.ShouldBe(minThroughput);
            throughputProperties.AutoscaleMaxThroughput.ShouldBe(maxThroughput);
        }

        public class DummyDatabase : CosmosDatabase
        {
            public DummyDatabase(IDatabaseOptions options) : base(options, true) { }

            public required ICosmosDbCollection<DummyEntity> Dummies { get; set; }
        }

        public class DummyEntity : ICosmosEntity
        {
            public required string Id { get; set; }
        }
    }
}
