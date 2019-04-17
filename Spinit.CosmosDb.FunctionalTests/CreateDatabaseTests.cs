using System;
using System.Threading.Tasks;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class CreateDatabaseTests : IClassFixture<CreateDatabaseTests.DummyDatabase>
    {
        private readonly DummyDatabase _database;

        public CreateDatabaseTests(DummyDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCreateDatabase()
        {
            await _database.Operations.CreateIfNotExistsAsync();
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestDeleteDatabase()
        {
            await _database.Operations.DeleteAsync();
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
