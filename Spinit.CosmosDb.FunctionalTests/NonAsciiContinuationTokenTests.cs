using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    public class NonAsciiContinuationTokenTests
    {
        [Fact]
        public async Task ShouldBeAbleToIterateOverAllPages()
        {
            var database = new TestDatabase();
            await database.Operations.CreateIfNotExistsAsync();
            try
            {
                var entityCount = 100;
                var entities = Enumerable.Range(1, entityCount).Select(x => new TestEntity { Id = x.ToString(), Title = "åäö" });
                foreach (var entity in entities)
                {
                    await database.TestEntities.UpsertAsync(entity);
                }

                string continuationToken = null;
                var fetchedItems = 0;
                do
                {
                    var searchRequest = new SearchRequest<TestEntity>
                    {
                        ContinuationToken = continuationToken,
                        PageSize = 1,
                        SortBy = x => x.Title
                    };
                    var searchResponse = await database.TestEntities.SearchAsync(searchRequest);
                    continuationToken = searchResponse.ContinuationToken;
                    fetchedItems += searchResponse.Documents.Count();

                } while (continuationToken != null);
                Assert.Equal(entityCount, fetchedItems);
            }
            finally
            {
                await database.Operations.DeleteAsync();
            }
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionalTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
            }

            public void Dispose()
            {
                if (FunctionalTestsConfiguration.Enabled)
                    Operations.DeleteAsync().GetAwaiter().GetResult();
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
