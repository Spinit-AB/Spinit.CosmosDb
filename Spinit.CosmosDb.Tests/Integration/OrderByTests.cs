using System;
using System.Linq;
using System.Threading.Tasks;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    [TestCaseOrderer("Spinit.CosmosDb.Tests.Core.Order.TestCaseByAttributeOrderer", "Spinit.CosmosDb.Tests")]
    public class OrderByTests : DatabaseTestBase<OrderByTests.TestDatabase>
    {
        public OrderByTests(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture) { }


        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            foreach (var entity in GetEntities().Select(x => (TestEntity)x.First()))
            {
                await Database.TestEntities.UpsertAsync(entity);
            }
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetEntities))]
        public async Task OrderShouldBeAsExcpected(TestEntity entity)
        {
            var result = await Database.TestEntities.SearchAsync(new SearchRequest<TestEntity> { SortBy = x => x.Title, SortDirection = SortDirection.Ascending });
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

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options) : base(options, initialize: true) { }

            public ICosmosDbCollection<TestEntity> TestEntities { get; set; }
        }

        public class TestEntity : ICosmosEntity
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
    }
}
