using System;
using System.Threading.Tasks;
using Spinit.CosmosDb.Tests.Core;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    public class CustomAnalyzerTests : DatabaseTestBase<CustomAnalyzerTests.TestDatabase>
    {
        public CustomAnalyzerTests(CosmosDatabaseFixture<TestDatabase> cosmosDatabaseFixture) : base(cosmosDatabaseFixture)
        {
        }

        [Fact]
        public async Task TestCustomAnalyser()
        {
            await Database.Entities.UpsertAsync(new TestEntity { Id = "123", Title = "Test title for entity 1" });
            var response = await Database.Entities.SearchAsync(new SearchRequest<TestEntity> { Query = "ti" });
            Assert.Single(response.Documents);
        }

        public class TestDatabase : CosmosDatabase<TestDatabase>
        {
            public TestDatabase(DatabaseOptions<TestDatabase> options)
                : base(options, initialize: true)
            { }

            public ICosmosDbCollection<TestEntity> Entities { get; set; } = default!;

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
            public required string Id { get; set; }
            public string? Title { get; set; }
        }
    }
}
