﻿using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
{
    public class CosmosDbCollectionTests
    {
        public class WhenSearchingFullPageShouldBeReturnedIfAvailable
        {
            [Fact]
            public async Task ShouldReturnFullPage()
            {
                var collection = new MockDbCollectionThatOnlyReturnsOneRecordOnSearch();
                var searchResponse = await collection.SearchAsync(new SearchRequest<DummyEntity> { PageSize = 100 });
                searchResponse.Documents.Count().ShouldBe(MockDbCollectionThatOnlyReturnsOneRecordOnSearch.Data.Count());
            }

            internal class MockDbCollectionThatOnlyReturnsOneRecordOnSearch : CosmosDbCollection<DummyEntity>
            {
                public static IEnumerable<DummyEntity> Data { get; } = Enumerable.Range(1, 10).Select(x => new DummyEntity { Id = x.ToString() }).ToList();

                public MockDbCollectionThatOnlyReturnsOneRecordOnSearch()
                    : base(null, null, new CollectionModel())
                {
                }

                protected internal override Task<SearchResponse<TProjection>> ExecuteSearchAsync<TProjection>(ISearchRequest<DummyEntity> request)
                {
                    var skip = string.IsNullOrEmpty(request.ContinuationToken)
                        ? 0
                        : int.Parse(request.ContinuationToken);
                    var item = Data.Skip(skip).First();
                    var continuationToken = item == Data.Last()
                        ? null
                        : item.Id;
                    var searchResponse = new SearchResponse<TProjection>
                    {
                        Documents = new[] { JsonConvert.DeserializeObject<TProjection>(JsonConvert.SerializeObject(item)) }!,
                        ContinuationToken = continuationToken
                    };
                    return Task.FromResult(searchResponse);
                }
            }

            public class DummyEntity : ICosmosEntity
            {
                public required string Id { get; set; }
            }
        }
    }
}
