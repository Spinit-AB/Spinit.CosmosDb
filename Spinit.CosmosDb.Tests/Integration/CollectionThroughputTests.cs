﻿using Microsoft.Azure.Cosmos;
using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    [TestCaseOrderer("Spinit.CosmosDb.Tests.Core.Order.TestCaseByAttributeOrderer", "Spinit.CosmosDb.Tests")]
    [Collection("DatabaseIntegrationTest")]
    public class CollectionThroughputTests : IClassFixture<CollectionThroughputTests.TestDatabaseFixture>
    {
        private readonly TestDatabase _database;

        public CollectionThroughputTests(TestDatabaseFixture fixture)
        {
            _database = fixture.Database;
        }

        [Fact]
        [TestOrder]
        public async Task CreateDatabaseWithContainerThroughput()
        {
            await _database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(1000, ThroughputType.Container));
        }

        [Fact]
        [TestOrder]
        public async Task TestReadCollectionThroughput()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            throughputProperties.Throughput.ShouldBe(1000);
        }

        [Fact]
        [TestOrder]
        public async Task TestSetDatabaseThroughput()
        {
            await _database.Todos.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(400));
        }

        [Fact]
        [TestOrder]
        public async Task TestGetDatabaseThroughputAfterSet()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            throughputProperties.Throughput.ShouldBe(400);
        }


        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(IDatabaseOptions options) : base(options) { }

            public required ICosmosDbCollection<TodoItem> Todos { get; set; }
        }

        public class TodoItem : ICosmosEntity
        {
            public required string Id { get; set; }
        }

        public class TestDatabaseFixture : CosmosDatabaseFixture<TestDatabase>
        {
            public TestDatabaseFixture(DatabaseFixture databaseFixture) : base(databaseFixture)
            {
            }

            // Do not create database in this step
            public override Task InitializeAsync() => Task.CompletedTask;
        }
    }
}
