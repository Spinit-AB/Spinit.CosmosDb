﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Spinit.CosmosDb.Tests.Core;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    public class NonAsciiContinuationTokenTests : DatabaseTestBase<NonAsciiContinuationTokenTests.TestDatabase>
    {
        public NonAsciiContinuationTokenTests(CosmosDatabaseFixture<TestDatabase> cosmosDbFixture) : base(cosmosDbFixture) { }

        [Fact]
        public async Task ShouldBeAbleToIterateOverAllPages()
        {
            var entityCount = 100;
            var entities = Enumerable.Range(1, entityCount).Select(x => new TestEntity { Id = x.ToString(), Title = "åäö" });
            await Database.TestEntities.UpsertAsync(entities);

            string continuationToken = null;
            var fetchedItems = 0;
            do
            {
                var searchRequest = new SearchRequest<TestEntity>
                {
                    ContinuationToken = continuationToken,
                    PageSize = 10,
                    SortBy = x => x.Title
                };
                var searchResponse = await Database.TestEntities.SearchAsync(searchRequest);
                continuationToken = searchResponse.ContinuationToken;
                fetchedItems += searchResponse.Documents.Count();

            } while (continuationToken != null);
            Assert.Equal(entityCount, fetchedItems);
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