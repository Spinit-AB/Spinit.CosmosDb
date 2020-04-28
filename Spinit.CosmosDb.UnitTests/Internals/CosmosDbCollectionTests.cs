using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Internals
{
    public class CosmosDbCollectionTests
    {
        //public class WhenGettingDbEntryShouldBeUsed
        //{
        //    [Fact]
        //    public async Task GetShouldCallReadDocumentAsyncWithDbEntry()
        //    {
        //        var id = "123456";
        //        var container = Mock.Of<Container>();
        //        Mock.Get(container)
        //            .DefaultValueProvider = new DocumentResponseValueProvider();
        //        Mock.Get(container)
        //            .Setup(x => x.ReadItemAsync<DbEntry<TestEntity>>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
        //            .ReturnsAsync(() =>
        //            {
        //                return (ItemResponse<DbEntry<TestEntity>>) new DbEntry<TestEntity>() { Original = new TestEntity { Id = id, Title = "Title" } };
        //            })
        //            .Verifiable("ReadDocumentAsync not called with DbEntry<TestEntity>");
        //        var model = new CollectionModel
        //        {
        //            DatabaseId = "Database",
        //            CollectionId = "Collection"
        //        };
        //        var collection = new CosmosDbCollection<TestEntity>(container, model);
        //        var response = await collection.GetAsync(id);
        //        Mock.Get(container).Verify();
        //    }

        //    [Fact]
        //    public async Task GetWithProjectionShouldCallReadDocumentAsyncWithDbEntry()
        //    {
        //        var id = "123456";
        //        var documentClientMock = new Mock<IDocumentClient> { DefaultValue = DefaultValue.Mock, DefaultValueProvider = new DocumentResponseValueProvider() };
        //        documentClientMock
        //           .Setup(x => x.ReadDocumentAsync<DbEntry<TestEntity>>(It.IsAny<Uri>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
        //           .ReturnsAsync(() =>
        //           {
        //               return new DocumentResponse<DbEntry<TestEntity>>(new DbEntry<TestEntity>() { Original = new TestEntity { Id = id, Title = "Title" } });
        //           })
        //           .Verifiable("ReadDocumentAsync not called with DbEntry<TestEntity>");
        //        var documentClient = documentClientMock.Object;
        //        var model = new CollectionModel
        //        {
        //            DatabaseId = "Database",
        //            CollectionId = "Collection"
        //        };
        //        var collection = new CosmosDbCollection<TestEntity>(documentClient, model);
        //        var response = await collection.GetAsync<TestEntity>(id);
        //        Mock.Get(documentClient).Verify();
        //    }

        //    public class TestEntity : ICosmosEntity
        //    {
        //        public string Id { get; set; }
        //        public string Title { get; set; }
        //    }

        //    public class DocumentResponseValueProvider : LookupOrFallbackDefaultValueProvider
        //    {
        //        protected override object GetFallbackDefaultValue(Type type, Mock mock)
        //        {
        //            var result = base.GetFallbackDefaultValue(type, mock);
        //            if (result == null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DocumentResponse<>))
        //            {
        //                var documentType = type.GenericTypeArguments.First();
        //                var document = Activator.CreateInstance(documentType);
        //                var documentResponseType = typeof(DocumentResponse<>).MakeGenericType(documentType);
        //                result = Activator.CreateInstance(documentResponseType, document);
        //                return result;
        //            }
        //            return result;
        //        }
        //    }
        //}

        public class WhenSearchingFullPageShouldBeReturnedIfAvailable
        {
            [Fact]
            public async Task ShouldReturnFullPage()
            {
                var collection = new MockDbCollectionThatOnlyReturnsOneRecordOnSearch();
                var searchResponse = await collection.SearchAsync(new SearchRequest<DummyEntity> { PageSize = 100 });
                Assert.Equal(MockDbCollectionThatOnlyReturnsOneRecordOnSearch.Data.Count(), searchResponse.Documents.Count());
            }

            internal class MockDbCollectionThatOnlyReturnsOneRecordOnSearch : CosmosDbCollection<DummyEntity>
            {
                public static IEnumerable<DummyEntity> Data { get; } = Enumerable.Range(1, 10).Select(x => new DummyEntity { Id = x.ToString() }).ToList();

                public MockDbCollectionThatOnlyReturnsOneRecordOnSearch()
                    : base(null, new CollectionModel())
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
                        Documents = new[] { JsonConvert.DeserializeObject<TProjection>(JsonConvert.SerializeObject(item)) },
                        ContinuationToken = continuationToken
                    };
                    return Task.FromResult(searchResponse);
                }
            }

            public class DummyEntity : ICosmosEntity
            {
                public string Id { get; set; }
            }
        }
    }
}
