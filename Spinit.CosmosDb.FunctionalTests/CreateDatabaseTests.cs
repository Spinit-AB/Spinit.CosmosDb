using System;
using System.Threading.Tasks;
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
            var throughput = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(400, throughput);
        }
        
        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithtDefaultContainerThroughputSet(int defaultThroughput, int expected)
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Container));
            var throughput = await database.Dummies.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(expected, throughput);
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
        [InlineData(500, 500)]
        [InlineData(1000, 1000)]
        public async Task TestCreateDatabaseWithDefaultDatabaseThroughputSet(int defaultThroughput, int expected)
        {
            var database = new DummyDatabase();

            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(defaultThroughput, ThroughputType.Database));
            var throughput = await database.Operations.GetThroughputAsync();
            await database.Operations.DeleteAsync();

            Assert.Equal(expected, throughput);
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
