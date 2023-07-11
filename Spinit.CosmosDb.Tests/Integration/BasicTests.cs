using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    public class BasicIntegrationTests : DatabaseTestBase<BasicIntegrationTests.TestDatabase>
    {
        public BasicIntegrationTests(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture) { }

        [Fact]
        public async Task Create()
        {
            var entity = new TestEntity { Id = "id", Text = "Some text" };
            await Database.TestEntitites.UpsertAsync(entity);

            var read_entity = await Database.TestEntitites.GetAsync("id");
            read_entity.ShouldBeEquivalentTo(entity);
        }

        [Fact]
        public async Task Upsert()
        {
            var entity = new TestEntity { Id = "id", Text = "Some text" };
            await Database.TestEntitites.UpsertAsync(entity);

            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "id", Text = "Some other text" });

            var read_entity = await Database.TestEntitites.GetAsync("id");
            read_entity.Text.ShouldBeEquivalentTo("Some other text");
        }

        [Fact]
        public async Task Delete()
        {
            var entity = new TestEntity { Id = "id", Text = "Some text" };
            await Database.TestEntitites.UpsertAsync(entity);

            await Database.TestEntitites.DeleteAsync("id");

            var searchResults = await Database.TestEntitites.SearchAsync(new SearchRequest<TestEntity>());
            searchResults.Documents.ShouldBeEmpty();
        }

        [Fact]
        public async Task Delete_Bulk()
        {
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "first", Text = "First text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "second", Text = "Second text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "third", Text = "Third text" });

            await Database.TestEntitites.DeleteAsync(new[] { "first", "second", "third" });

            var all_items = await Database.TestEntitites.SearchAsync(new SearchRequest<TestEntity>());
            all_items.Documents.ShouldBeEmpty();
        }

        [Fact]
        public async Task Count()
        {
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "first", Text = "First text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "second", Text = "Second text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "third", Text = "Third text" });

            var count_all = await Database.TestEntitites.CountAsync(new SearchRequest<TestEntity>());
            count_all.ShouldBe(3);

            var count_specific = await Database.TestEntitites.CountAsync(new SearchRequest<TestEntity>
            {
                Filter = (f) => f.Text.Contains("First")
            });
            count_specific.ShouldBe(1);
        }

        [Fact]
        public async Task Update_Bulk()
        {
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "first", Text = "First text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "second", Text = "Second text" });
            await Database.TestEntitites.UpsertAsync(new TestEntity { Id = "third", Text = "Third text" });

            await Database.TestEntitites.UpsertAsync(new[]
            {
                new TestEntity { Id = "first", Text = "First updated text" },
                new TestEntity { Id = "second", Text = "Second updated text" },
                new TestEntity { Id = "third", Text = "Third updated text" }
            });

            var all_items = await Database.TestEntitites.SearchAsync(new SearchRequest<TestEntity>());

            all_items.Documents.ShouldAllBe(x => x.Text.Contains("updated"));
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options)
                : base(options, initialize: true) { }

            public ICosmosDbCollection<TestEntity> TestEntitites { get; private set; } = default!;
        }

        public class TestEntity : ICosmosEntity
        {
            public required string Id { get; set; }
            public required string Text { get; set; }
            public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        }
    }
}
