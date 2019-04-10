using System;
using System.Linq;
using System.Threading.Tasks;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class OrderByTests : IClassFixture<OrderByTests.TestDatabase>
    {
        private const string ShouldSkip =
#if DEBUG
            null;
#else
            "Functional test should only run locally in debug mode";
#endif

        private readonly TestDatabase _database;

        public OrderByTests(TestDatabase database)
        {
            _database = database;
            _database.Database.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            foreach (var entity in GetEntities().Select(x => (TestEntity)x.First()))
            {
                _database.TestEntities.UpsertAsync(entity).GetAwaiter().GetResult();
            }
        }       

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetEntities))]
        public async Task OrderShouldBeAsExcpected(TestEntity entity)
        {
            var result = await _database.TestEntities.SearchAsync(new SearchRequest<TestEntity> { SortBy = x => x.Title, SortDirection = SortDirection.Ascending });
            var index = result.Documents.ToList().FindIndex(x => x.Id == entity.Id);
            if (entity.Title.ToLower() == "a")
                Assert.InRange(index, 0, 1);
            else if (entity.Title.ToLower() == "b")
                Assert.InRange(index, 2, 3);
        }

        public static TheoryData<TestEntity> GetEntities()
        {
            var result = new TheoryData<TestEntity>
            {
                new TestEntity
                {
                    Id = "1",
                    Title = "a"
                },
                new TestEntity
                {
                    Id = "2",
                    Title = "A"
                },
                new TestEntity
                {
                    Id = "3",
                    Title = "b"
                },
                new TestEntity
                {
                    Id = "4",
                    Title = "B"
                }
            };
            return result;
        }

        public class TestDatabase : CosmosDatabase, IDisposable
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;DatabaseId={databaseId}";
            }

            public void Dispose()
            {
                Database.DeleteAsync().GetAwaiter().GetResult();
            }

            public ICosmosDbCollection<TestEntity> TestEntities { get; set; }
        }

        public class TestEntity : ICosmosEntity
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
    }
}
