using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Spinit.Expressions;
using System.IO;
using Newtonsoft.Json;
using Spinit.CosmosDb.Validation;
using System.Diagnostics;
using Spinit.CosmosDb.Internals;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Spinit.CosmosDb
{
    internal class CosmosStreamResponse<T>
        where T : class, ICosmosEntity
    {
        public IEnumerable<DbEntry<T>> Documents { get; set; }

        [JsonProperty("_count")]
        public int Count { get; set; }
    }

    internal class CosmosDbCollection<TEntity> : ICosmosDbCollection<TEntity>
        where TEntity : class, ICosmosEntity
    {
        private readonly Container _container;
        private readonly Container _bulkContainer;
        private readonly CollectionModel _model;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public CosmosDbCollection(Container container, Container bulkContainer, CollectionModel model, JsonSerializerSettings settings = null)
        {
            _container = container;
            _bulkContainer = bulkContainer;
            _model = model;
            _jsonSerializerSettings = settings ?? new JsonSerializerSettings();
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task<ICosmosBulkOperationResult> UpsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var bulkOperations = new Bulk.Operations<TEntity>(entities.Count());
            
            foreach (var entity in entities)
            {
                var entry = new DbEntry<TEntity>(entity, _model.Analyzer, _jsonSerializerSettings);
                bulkOperations.Add(Bulk.CaptureOperationResponse(_bulkContainer.UpsertItemAsync(entry, new PartitionKey(entry.Id), cancellationToken: cancellationToken), entry, EntryToEntityTransformation));
            }

            var result = await bulkOperations.ExecuteAsync().ConfigureAwait(false);
            if (result.Failures is { Count: > 0 })
            {
                throw SpinitCosmosDbBulkException.Create("upsert", result);
            }

            return result;
        }

        public Task UpsertAsync(TEntity document)
        {
            // TODO: handle HTTP 429 (Too Many Requests) errors
            var entry = new DbEntry<TEntity>(document, _model.Analyzer, _jsonSerializerSettings);

            return _container.UpsertItemAsync(entry, new PartitionKey(entry.Id));
        }

        public Task DeleteAsync(string id)
        {
            return _container.DeleteItemAsync<TEntity>(id, new PartitionKey(id));
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task<ICosmosBulkOperationResult> DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            var bulkOperations = new Bulk.Operations<string>(ids.Count());

            foreach (var id in ids)
            {
                bulkOperations.Add(Bulk.CaptureOperationResponse(_bulkContainer.DeleteItemAsync<string>(id, new PartitionKey(id), cancellationToken: cancellationToken), id));
            }

            var result = await bulkOperations.ExecuteAsync().ConfigureAwait(false);
            if (result.Failures is { Count: > 0 })
            {
                throw SpinitCosmosDbBulkException.Create("delete", result);
            }

            return result;
        }

        public async Task<int> CountAsync(ISearchRequest<TEntity> request)
        {
            var query = CreateQuery(request);
            var result = await query.CountAsync().ConfigureAwait(false);

            return result.Resource;
        }

        public async Task<ThroughputProperties> GetThroughputAsync()
        {
            return await _container.ReadThroughputAsync(new RequestOptions()).ConfigureAwait(false);
        }

        public Task SetThroughputAsync(ThroughputProperties throughputProperties)
        {
            if (throughputProperties is { } && !ThroughputValidator.IsValidThroughput(throughputProperties, out var message))
            {
                throw new ArgumentException(message, nameof(throughputProperties));
            }

            return _container.ReplaceThroughputAsync(throughputProperties);
        }

        internal protected virtual async Task<SearchResponse<TProjection>> ExecuteSearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity
        {
            var query = CreateQuery(request);

            var queryDefinition = query.ToQueryDefinition();

            var iterator = _container.GetItemQueryStreamIterator(queryDefinition, request.ContinuationToken, new QueryRequestOptions { MaxItemCount = request.PageSize });

            using var streamResponse = await iterator.ReadNextAsync().ConfigureAwait(false);
            if (streamResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new SpinitCosmosDbException(streamResponse.StatusCode, streamResponse.ErrorMessage);
            }

            using var streamReader = new StreamReader(streamResponse.Content);
            var response = JsonConvert.DeserializeObject<CosmosStreamResponse<TProjection>>(streamReader.ReadToEnd(), _jsonSerializerSettings);

            return new SearchResponse<TProjection>
            {
                ContinuationToken = streamResponse.ContinuationToken,
                Documents = response.Documents?.Select(x => x.Original),
                TotalCount = request.IncludeTotalCount
                    ? await query.CountAsync().ConfigureAwait(false)
                    : (int?)null
            };
        }

        private IQueryable<DbEntry<TEntity>> CreateQuery(ISearchRequest<TEntity> request)
        {
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

            return query;
        }

        private static TEntity EntryToEntityTransformation(DbEntry<TEntity> entry)
        {
            return entry.Original;
        }
    }
}
