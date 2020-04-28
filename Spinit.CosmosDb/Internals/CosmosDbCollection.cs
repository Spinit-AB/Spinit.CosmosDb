using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Spinit.Expressions;

namespace Spinit.CosmosDb
{
    internal class CosmosDbCollection<TEntity> : ICosmosDbCollection<TEntity>
        where TEntity : class, ICosmosEntity
    {
        private readonly Container _container;
        private readonly CollectionModel _model;

        public CosmosDbCollection(Container container, CollectionModel model)
        {
            _container = container;
            _model = model;
        }

        public Task<SearchResponse<TEntity>> SearchAsync(ISearchRequest<TEntity> request) => SearchAsync<TEntity>(request);

        public async Task<SearchResponse<TProjection>> SearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity
        {
            var searchResponse = await ExecuteSearchAsync<TProjection>(request).ConfigureAwait(false);

            if (request.PageSize.HasValue && searchResponse.Documents.Count() < request.PageSize && !string.IsNullOrEmpty(searchResponse.ContinuationToken))
            {
                var innerSearchRequest = new SearchRequest<TEntity>().Assign(request);
                // skip CotalCount, if set it should already been calculated
                innerSearchRequest.IncludeTotalCount = false;

                innerSearchRequest.ContinuationToken = searchResponse.ContinuationToken;
                innerSearchRequest.PageSize = request.PageSize - searchResponse.Documents.Count();

                var innerSearchResponse = await SearchAsync<TProjection>(innerSearchRequest).ConfigureAwait(false);
                innerSearchResponse.Documents = searchResponse.Documents.Union(innerSearchResponse.Documents);
                innerSearchResponse.TotalCount = searchResponse.TotalCount;
                return innerSearchResponse;
            }

            return searchResponse;
        }

        public Task<TEntity> GetAsync(string id) => GetAsync<TEntity>(id);

        public async Task<TProjection> GetAsync<TProjection>(string id)
            where TProjection : class, ICosmosEntity
        {
            try
            {
                var response = await _container.ReadItemAsync<DbEntry<TProjection>>(id, new PartitionKey(id)).ConfigureAwait(false);

                return response.Resource.Original;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task UpsertAsync(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
            //    var documentCollection = await _documentClient.ReadDocumentCollectionAsync(GetCollectionUri()).ConfigureAwait(false);

            //    var bulkExecutor = new BulkExecutor(_documentClient as DocumentClient, documentCollection);
            //    await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            //    var entries = entities.Select(x => new DbEntry<TEntity>(x, _model.Analyzer));

            //    BulkImportResponse bulkImportResponse = null;
            //    do
            //    {
            //        bulkImportResponse = await bulkExecutor
            //            .BulkImportAsync(
            //                entries,
            //                enableUpsert: true,
            //                disableAutomaticIdGeneration: true)
            //            .ConfigureAwait(false);
            //    } while (bulkImportResponse.NumberOfDocumentsImported < entries.Count());
        }

    public Task UpsertAsync(TEntity document)
        {
            try
            {
                var entry = new DbEntry<TEntity>(document, _model.Analyzer);

                return _container.UpsertItemAsync(entry, new PartitionKey(entry.Id));
            }
            catch (CosmosException)
            {
                // TODO: handle HTTP 429 (Too Many Requests) errors
                throw;
            }
        }

        public Task DeleteAsync(string id)
        {
            return _container.DeleteItemAsync<TEntity>(id, new PartitionKey(id));
        }

        public Task DeleteAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
            //    var documentCollection = await _documentClient.ReadDocumentCollectionAsync(GetCollectionUri()).ConfigureAwait(false);

            //    var bulkExecutor = new BulkExecutor(_documentClient as DocumentClient, documentCollection);
            //    await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            //    var entries = ids.Select(x => new Tuple<string, string>(x, x)).ToList();

            //    BulkDeleteResponse bulkDeleteResponse = null;
            //    do
            //    {
            //        bulkDeleteResponse = await bulkExecutor
            //            .BulkDeleteAsync(entries)
            //            .ConfigureAwait(false);
            //    } while (bulkDeleteResponse.NumberOfDocumentsDeleted < entries.Count && bulkDeleteResponse.NumberOfDocumentsDeleted > 0);
        }

        internal protected virtual async Task<SearchResponse<TProjection>> ExecuteSearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity
        {
            //var feedOptions = new FeedOptions
            //{
            //    EnableCrossPartitionQuery = true,
            //    MaxItemCount = request.PageSize,
            //    RequestContinuation = request.ContinuationToken
            //};
            //var query = _cosmosClient.CreateDocumentQuery<DbEntry<TEntity>>(GetCollectionUri(), feedOptions).AsQueryable();

            var query = _container.GetItemLinqQueryable<DbEntry<TEntity>>(
                    continuationToken: request.ContinuationToken,
                    requestOptions: new QueryRequestOptions { MaxItemCount = request.PageSize })
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Query))
            {
                var tokens = _model.Analyzer.AnalyzeQuery(request.Query);
                foreach (var token in tokens)
                    query = query.Where(x => x.All.Contains(token));
            }

            if (request.Filter != null)
            {
                query = query.Where(request.Filter.RemapTo<DbEntry<TEntity>, TEntity, bool>(x => x.Original));
            }

            if (request.SortBy != null)
            {
                query = request.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(request.SortBy.RemapTo<DbEntry<TEntity>, TEntity, object>(x => x.Normalized))
                    : query.OrderByDescending(request.SortBy.RemapTo<DbEntry<TEntity>, TEntity, object>(x => x.Normalized));
            }

            var feedResponse = await query
                .Select(x => x.Original)
                .Cast<TProjection>()
                .ToFeedIterator()
                .ReadNextAsync()
                .ConfigureAwait(false);
            //var feedResponse = await query.Select(x => x.Original).AsDocumentQuery().ExecuteNextAsync<TProjection>().ConfigureAwait(false);

            return new SearchResponse<TProjection>
            {
                ContinuationToken = EncodeContinuationToken(feedResponse.ContinuationToken),
                Documents = feedResponse.ToArray(),
                TotalCount = request.IncludeTotalCount
                    ? await query.CountAsync().ConfigureAwait(false)
                    : (int?)null
            };
        }

        //private Uri GetDocumentUri(string id)
        //{
        //    return UriFactory.CreateDocumentUri(_model.DatabaseId, _model.CollectionId, id);
        //}

        //private Uri GetCollectionUri()
        //{
        //    return UriFactory.CreateDocumentCollectionUri(_model.DatabaseId, _model.CollectionId);
        //}

        private string EncodeContinuationToken(string continuationToken)
        {
            if (string.IsNullOrEmpty(continuationToken))
                return continuationToken;

            return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(continuationToken));
        }
    }
}
