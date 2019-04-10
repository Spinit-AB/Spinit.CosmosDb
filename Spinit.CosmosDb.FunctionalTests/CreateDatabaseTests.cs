using System;
using System.Threading.Tasks;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class CreateDatabaseTests : IClassFixture<CreateDatabaseTests.DummyDatabase>
    {
        private const string ShouldSkip =
#if DEBUG
            null;
#else
            "Functional test should only run locally in debug mode";
#endif

        private readonly DummyDatabase _database;

        public CreateDatabaseTests(DummyDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = ShouldSkip)]
        [TestOrder]
        public async Task TestCreateDatabase()
        {
            await _database.Database.CreateIfNotExistsAsync();
        }

        [Fact(Skip = ShouldSkip)]
        [TestOrder]
        public async Task TestDeleteDatabase()
        {
            await _database.Database.DeleteAsync();
        }

        public class DummyDatabase : CosmosDatabase
        {
            public DummyDatabase()
                : base(new DatabaseOptionsBuilder<DummyDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;DatabaseId={databaseId}";
            }

            public ICosmosDbCollection<DummyEntity> Dummies { get; set; }
        }

        public class DummyEntity : ICosmosEntity
        {
            public string Id { get; set; }
        }
    }
}
