using System;
using System.Threading.Tasks;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    public class CustomAnalyzerTests
    {
        [Fact]
        public async Task TestCustomAnalyser()
        {
            var database = new TestDatabase();
            await database.Operations.CreateIfNotExistsAsync();
            try
            {
                await database.Entities.UpsertAsync(new TestEntity { Id = "123", Title = "Test title for entity 1" });
                var response = await database.Entities.SearchAsync(new SearchRequest<TestEntity> { Query = "ti" });
                Assert.Single(response.Documents);
            }
            finally
            {
                await database.Operations.DeleteAsync();
            }
        }

        public class TestDatabase : CosmosDatabase<TestDatabase>
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionalTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
            }

            public ICosmosDbCollection<TestEntity> Entities { get; set; }

            protected override void OnModelCreating(DatabaseModelBuilder<TestDatabase> modelBuilder)
            {
                modelBuilder.Collection(x => x.Entities)
                    .Analyzer(analyzer => analyzer
                        .TokenFilters
                            .Add<EdgeNGramTokenFilter>());
            }
        }

        public class TestEntity : ICosmosEntity
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
    }
}
