﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class FunctionalDatabaseThroughputTests : IClassFixture<FunctionalDatabaseThroughputTests.TestDatabase>
    {
        private readonly TestDatabase _database;

        public FunctionalDatabaseThroughputTests(TestDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task CreateDatabaseWithDatabaseThroughput()
        {
            await _database.Operations.CreateIfNotExistsAsync(new CreateDbOptions(1000, ThroughputType.Database));
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReadDatabaseThroughput()
        {
            var throughputProperties = await _database.Operations.GetThroughputAsync();
            Assert.Equal(1000, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestSetDatabaseThroughput()
        {
            await _database.Operations.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(400));
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestGetDatabaseThroughputAfterSet()
        {
            var throughputProperties = await _database.Operations.GetThroughputAsync();
            Assert.Equal(400, throughputProperties.Throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestDeleteDatabase()
        {
            await _database.Operations.DeleteAsync();
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
            }

        }
    }
}
