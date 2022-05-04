using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class CreateDatabaseTests : IClassFixture<CreateDatabaseTests.DummyDatabase>
    {
        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCreateAndDeleteDatabase()
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync();
            await database.Operations.DeleteAsync();
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCreateDatabaseWithoutDefaultThroughputSet()
        {
            var database = new DummyDatabase();
            
            await database.Operations.CreateIfNotExistsAsync();
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(400, throughputProperties.Throughput);
        }
        
        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithtDefaultContainerThroughputSet(int defaultThroughput, int expected)
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(expected, throughputProperties.Throughput);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [InlineData(399)]
        [InlineData(10001)]
        [InlineData(550)]
        [InlineData(999)]
        public async Task TestCreateDatabaseWithInvalidThroughputSet(int defaultThroughput)
        {
            var database = new DummyDatabase();

            await Assert.ThrowsAsync<ArgumentException>(async () => await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Container)));
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [InlineData(400)]
        [InlineData(3999)]
        [InlineData(4500)]
        [InlineData(1001000)]
        public async Task TestCreateDatabaseWithInvalidAutoscaleThroughputSet(int maxThroughput)
        {
            var database = new DummyDatabase();

            await Assert.ThrowsAsync<ArgumentException>(async () => await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateAutoscaleThroughput(maxThroughput), ThroughputType.Container)));
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithDefaultDatabaseThroughputSet(int defaultThroughput, int expected)
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Database));
            var throughputProperties = await database.Operations.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(expected, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCreateDatabaseWithCustomContainerThroughputSet()
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateManualThroughput(500), ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(500, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCreateDatabaseWithAutoscaleContainerThroughputSet()
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(ThroughputProperties.CreateAutoscaleThroughput(4000), ThroughputType.Container));
            var throughputProperties = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(400, throughputProperties.Throughput);
            Assert.Equal(4000, throughputProperties.AutoscaleMaxThroughput);
        }

        public class DummyDatabase : CosmosDatabase
        {
            public DummyDatabase()
                : base(new DatabaseOptionsBuilder<DummyDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionTestsConfiguration.CosmosDbConnectionString};DatabaseId ={databaseId}";
            }

            public ICosmosDbCollection<DummyEntity> Dummies { get; set; }
        }

        public class DummyEntity : ICosmosEntity
        {
            public string Id { get; set; }
        }
    }
}
