using System;
using System.Threading.Tasks;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    public class DateTests
    {
        public class WhenUsingDateTime
        {
            [Collection(nameof(TestDatabaseCollection))]
            public class Utc
            {
                private readonly TestDatabase _database;
                private readonly TestEntity _input;

                public Utc(TestDatabase database)
                {
                    _database = database;
                    var utcNow = DateTime.UtcNow;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTime = utcNow
                    };

                    _database.TestEntities.UpsertAsync(_input).GetAwaiter().GetResult();
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task ValueShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTime, output.DateTime);
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task KindShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTime.Kind, output.DateTime.Kind);
                }
            }

            [Collection(nameof(TestDatabaseCollection))]
            public class Local
            {
                private readonly TestDatabase _database;
                private readonly TestEntity _input;

                public Local(TestDatabase database)
                {
                    _database = database;
                    var now = DateTime.Now;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTime = now
                    };

                    _database.TestEntities.UpsertAsync(_input).GetAwaiter().GetResult();
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task ValueShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTime, output.DateTime);
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task KindShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTime.Kind, output.DateTime.Kind);
                }
            }
        }

        public class WhenUsingDateTimeOffset
        {
            [Collection(nameof(TestDatabaseCollection))]
            public class Utc
            {
                private readonly TestDatabase _database;
                private readonly TestEntity _input;

                public Utc(TestDatabase database)
                {
                    _database = database;
                    var utcNow = DateTimeOffset.UtcNow;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = utcNow
                    };

                    _database.TestEntities.UpsertAsync(_input).GetAwaiter().GetResult();
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task ValueShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset, output.DateTimeOffset);
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset.Offset, output.DateTimeOffset.Offset);
                }
            }

            [Collection(nameof(TestDatabaseCollection))]
            public class Local
            {
                private readonly TestDatabase _database;
                private readonly TestEntity _input;

                public Local(TestDatabase database)
                {
                    _database = database;
                    var now = DateTimeOffset.Now;
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = now
                    };

                    _database.TestEntities.UpsertAsync(_input).GetAwaiter().GetResult();
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task ValueShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset, output.DateTimeOffset);
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset.Offset, output.DateTimeOffset.Offset);
                }
            }

            [Collection(nameof(TestDatabaseCollection))]
            public class PacificTimeZone
            {
                private readonly TestDatabase _database;
                private readonly TestEntity _input;

                public PacificTimeZone(TestDatabase database)
                {
                    _database = database;
                    var offset = TimeSpan.FromHours(8);
                    var dateTimeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    var now = new DateTimeOffset(dateTimeNow, offset);
                    _input = new TestEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        DateTimeOffset = now
                    };

                    _database.TestEntities.UpsertAsync(_input).GetAwaiter().GetResult();
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task ValueShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset, output.DateTimeOffset);
                }

                [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
                public async Task OffsetShouldBePreserved()
                {
                    var output = await _database.TestEntities.GetAsync(_input.Id);
                    Assert.Equal(_input.DateTimeOffset.Offset, output.DateTimeOffset.Offset);
                }
            }
        }

        [CollectionDefinition(nameof(TestDatabaseCollection))]
        public class TestDatabaseCollection : ICollectionFixture<TestDatabase> { }

        public class TestDatabase : CosmosDatabase, IDisposable
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            {
                if (FunctionTestsConfiguration.Enabled)
                    Operations.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
            }

            public void Dispose()
            {
                if (FunctionTestsConfiguration.Enabled)
                    Operations.DeleteAsync().GetAwaiter().GetResult();
            }

            public ICosmosDbCollection<TestEntity> TestEntities { get; set; }
        }

        public class TestEntity : ICosmosEntity
        {
            public string Id { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
        }
    }
}
