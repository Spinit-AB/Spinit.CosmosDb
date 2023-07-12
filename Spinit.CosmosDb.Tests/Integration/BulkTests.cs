using AutoFixture;
using Spinit.CosmosDb.Tests.Core;
using Xunit;
using Xunit.Abstractions;

namespace Spinit.CosmosDb.Tests.Integration
{
    [Collection("DatabaseIntegrationTest")]
    public class BulkTests : IAsyncLifetime
    {
        // These tests should be skipped by default since they are time consuming.
        public const string? SkipSlowBulkInsertsReason = "Skip slow bulk insert tests";
        private readonly ITestOutputHelper _output;
        

        public BulkTests(DatabaseFixture databaseFixture, ITestOutputHelper output)
        {
            DatabaseFixture = databaseFixture;
            _output = output;
        }

        protected DatabaseFixture DatabaseFixture { get; }

        public Task InitializeAsync() => Task.CompletedTask;

        [Theory]
        [InlineData(5_000)]
        [InlineData(500_000, Skip = SkipSlowBulkInsertsReason)]
        public async Task BulkInsert(int numberOfItems)
        {
            var fixture = new Fixture();
            var items = fixture.CreateMany<TestEntity>(numberOfItems);

            var database = DatabaseFixture.CreateCosmosDbDatabase<TestDatabase>();
            await database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(10_000, ThroughputType.Container));
            var response = await database.BulkInsertEntitites.UpsertAsync(items);
            _output.WriteLine($"Insertion succeeded:");
            _output.WriteLine($"\tDocuments inserted successfully: {response.SuccessfulDocuments:N0}");
            _output.WriteLine($"\tTotal execution time: {response.TotalTimeTaken.TotalSeconds:N2}s");
            _output.WriteLine($"\tExecution time per document: {response.TotalTimeTaken.TotalMicroseconds / numberOfItems:N2}µs");
            _output.WriteLine($"\tResources used: {response.TotalRequestUnitsConsumed:N0}ru");
        }

        [Theory]
        [InlineData(5_000)]
        [InlineData(500_000, Skip = SkipSlowBulkInsertsReason)]
        public async Task BulkInsert_With_AllowBulkExecution_Disabled(int numberOfItems)
        {
            var fixture = new Fixture();
            var items = fixture.CreateMany<TestEntity>(numberOfItems);

            var database = DatabaseFixture.CreateCosmosDbDatabase<TestDatabase>(x => x.Options.ConfigureCosmosClientOptions = setup =>
            {
                if (setup.BulkClient)
                {
                    setup.CosmosClientOptions.AllowBulkExecution = false;
                }
            });

            await database.Operations.CreateIfNotExistsAsync();

            try
            {
                var response = await database.BulkInsertEntitites.UpsertAsync(items);
                _output.WriteLine($"Insertion succeeded:");
                _output.WriteLine($"\tDocuments inserted successfully: {response.SuccessfulDocuments:N0}");
                _output.WriteLine($"\tTotal execution time: {response.TotalTimeTaken.TotalSeconds:N2}s");
                _output.WriteLine($"\tExecution time per document: {response.TotalTimeTaken.TotalMicroseconds / numberOfItems:N2}µs");
                _output.WriteLine($"\tResources used: {response.TotalRequestUnitsConsumed:N0}ru");
            }
            catch (SpinitCosmosDbBulkException bulkException)
            {
                _output.WriteLine($"Insertion failed:");
                _output.WriteLine($"\tDocuments inserted successfully: {bulkException.SuccessfulDocuments:N0}");
                _output.WriteLine($"\tDocument insertions failed: {bulkException.Failures.Count:N0}");
                _output.WriteLine($"\tTotal execution time: {bulkException.TotalTimeTaken.TotalSeconds:N2}s");
                _output.WriteLine($"\tExecution time per document: {bulkException.TotalTimeTaken.TotalMicroseconds / numberOfItems:N2}µs");
                _output.WriteLine($"\tResources used: {bulkException.TotalRequestUnitsConsumed:N0}ru");
            }
        }

        public Task DisposeAsync() => DatabaseFixture.Clear();

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options)
                : base(options, initialize: true) { }

            public ICosmosDbCollection<TestEntity> BulkInsertEntitites { get; private set; } = default!;
        }

        public class TestEntity : ICosmosEntity
        {
            public required string Id { get; set; }
            public required string Text { get; set; }
            public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        }
    }
}
