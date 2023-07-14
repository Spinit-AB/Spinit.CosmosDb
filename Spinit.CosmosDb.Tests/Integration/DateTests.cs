using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    public class DateTests
    {
        public class WhenUsingDateTime
        {
            public class Utc : DatabaseTestBase<TestDatabase>
            {
                private readonly TestEntity _input;

                public Utc(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture) 
                {
                    var utcNow = DateTime.UtcNow;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTime = utcNow
                    };
                }

                public override async Task InitializeAsync()
                {
                    await base.InitializeAsync();
                    await Database.TestEntities.UpsertAsync(_input);
                }

                [Fact]
                public async Task ValueShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTime.ShouldBe(_input.DateTime);
                }

                [Fact]
                public async Task KindShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTime.Kind.ShouldBe(_input.DateTime.Kind);
                }
            }


            public class Local : DatabaseTestBase<TestDatabase>
            {
                private readonly TestEntity _input;

                public Local(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture)
                {
                    var now = DateTime.Now;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTime = now
                    };
                }

                public override async Task InitializeAsync()
                {
                    await base.InitializeAsync();
                    await Database.TestEntities.UpsertAsync(_input);
                }

                [Fact]
                public async Task ValueShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTime.ShouldBe(_input.DateTime);
                }

                [Fact]
                public async Task KindShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTime.Kind.ShouldBe(_input.DateTime.Kind);
                }
            }
        }

        public class WhenUsingDateTimeOffset
        {
            public class Utc : DatabaseTestBase<TestDatabase>
            {
                private readonly TestEntity _input;

                public Utc(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture)
                {
                    var utcNow = DateTimeOffset.UtcNow;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = utcNow
                    };
                }

                public override async Task InitializeAsync()
                {
                    await base.InitializeAsync();
                    await Database.TestEntities.UpsertAsync(_input);
                }

                [Fact]
                public async Task ValueShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.ShouldBe(_input.DateTimeOffset);
                }

                [Fact]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.Offset.ShouldBe(_input.DateTimeOffset.Offset);
                }
            }

            public class Local : DatabaseTestBase<TestDatabase>
            {
                private readonly TestEntity _input;

                public Local(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture)
                {
                    var now = DateTimeOffset.Now;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = now
                    };
                }

                public override async Task InitializeAsync()
                {
                    await base.InitializeAsync();
                    await Database.TestEntities.UpsertAsync(_input);
                }

                [Fact]
                public async Task ValueShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.ShouldBe(_input.DateTimeOffset);
                }

                [Fact]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.Offset.ShouldBe(_input.DateTimeOffset.Offset);
                }
            }

            public class PacificTimeZone : DatabaseTestBase<TestDatabase>
            {
                private readonly TestEntity _input;

                public PacificTimeZone(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture)
                {
                    var offset = TimeSpan.FromHours(8);
                    var dateTimeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    var now = new DateTimeOffset(dateTimeNow, offset);
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = now
                    };
                }

                public override async Task InitializeAsync()
                {
                    await base.InitializeAsync();
                    await Database.TestEntities.UpsertAsync(_input);
                }

                [Fact]
                public async Task ValueShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.ShouldBe(_input.DateTimeOffset);
                }

                [Fact]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await Database.TestEntities.GetAsync(_input.Id);
                    output.DateTimeOffset.Offset.ShouldBe(_input.DateTimeOffset.Offset);
                }
            }
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(DatabaseOptions<TestDatabase> options)
                : base(options, initialize: true)
            { }

            public required ICosmosDbCollection<TestEntity> TestEntities { get; set; }
        }

        public class TestEntity : ICosmosEntity
        {
            public required string Id { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
        }
    }
}
