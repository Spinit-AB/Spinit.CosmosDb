using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Documents = Microsoft.Azure.Documents;
using Spinit.Expressions;
using System.IO;
using Newtonsoft.Json;
using Spinit.CosmosDb.Validation;

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
        private readonly Documents.IDocumentClient _documentClient;
        private readonly CollectionModel _model;

        public CosmosDbCollection(Container container, Documents.IDocumentClient documentClient, CollectionModel model)
        {
            _container = container;
            _documentClient = documentClient;
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

        public async Task UpsertAsync(IEnumerable<TEntity> entities)
        {
            var documentCollection = await _documentClient.ReadDocumentCollectionAsync(GetCollectionUri()).ConfigureAwait(false);

            var bulkExecutor = new BulkExecutor(_documentClient as Documents.Client.DocumentClient, documentCollection);
            await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            var entries = entities.Select(x => new DbEntry<TEntity>(x, _model.Analyzer));

            BulkImportResponse bulkImportResponse = null;
            do
            {
                bulkImportResponse = await bulkExecutor
                    .BulkImportAsync(
                        entries,
                        enableUpsert: true,
                        disableAutomaticIdGeneration: true)
                    .ConfigureAwait(false);
            } while (bulkImportResponse.NumberOfDocumentsImported < entries.Count());
        }

        public Task UpsertAsync(TEntity document)
        {
            // TODO: handle HTTP 429 (Too Many Requests) errors
            var entry = new DbEntry<TEntity>(document, _model.Analyzer);

            return _container.UpsertItemAsync(entry, new PartitionKey(entry.Id));
        }

        public Task DeleteAsync(string id)
        {
            return _container.DeleteItemAsync<TEntity>(id, new PartitionKey(id));
        }

        public async Task DeleteAsync(IEnumerable<string> ids)
        {
            var documentCollection = await _documentClient.ReadDocumentCollectionAsync(GetCollectionUri()).ConfigureAwait(false);

            var bulkExecutor = new BulkExecutor(_documentClient as Documents.Client.DocumentClient, documentCollection);
            await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            var entries = ids.Select(x => new Tuple<string, string>(x, x)).ToList();

            BulkDeleteResponse bulkDeleteResponse = null;
            do
            {
                bulkDeleteResponse = await bulkExecutor
                    .BulkDeleteAsync(entries)
                    .ConfigureAwait(false);
            } while (bulkDeleteResponse.NumberOfDocumentsDeleted < entries.Count && bulkDeleteResponse.NumberOfDocumentsDeleted > 0);
        }

        public async Task<int?> ReadThroughputAsync()
        {
            return await _container.ReadThroughputAsync().ConfigureAwait(false);
        }

        public async Task ReplaceThroughputAsync(int throughput)
        {
            if (!ThroughputValidator.IsValidThroughput(throughput))
            {
                throw new ArgumentException("The provided throughput is not valid. Must be between 400 and 1000000 and in increments of 100.", nameof(throughput));
            }

            await _container.ReplaceThroughputAsync(throughput).ConfigureAwait(false);
        }

        internal protected virtual async Task<SearchResponse<TProjection>> ExecuteSearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity
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

            var queryDefinition = query
                // Hack to get around bug: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/1438
                .Where(x => true)
                .ToQueryDefinition();
            var iterator = _container.GetItemQueryStreamIterator(queryDefinition, request.ContinuationToken, new QueryRequestOptions { MaxItemCount = request.PageSize });

            using var streamResponse = await iterator.ReadNextAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(streamResponse.Content);
            using var jsonTextReader = new JsonTextReader(streamReader);

            var response = JsonConvert.DeserializeObject<CosmosStreamResponse<TProjection>>(streamReader.ReadToEnd());

            return new SearchResponse<TProjection>
            {
                ContinuationToken = streamResponse.ContinuationToken,
                Documents = response.Documents?.Select(x => x.Original),
                TotalCount = request.IncludeTotalCount
                    ? await query.CountAsync().ConfigureAwait(false)
                    : (int?)null
            };
        }

        private Uri GetCollectionUri()
        {
            return Documents.Client.UriFactory.CreateDocumentCollectionUri(_model.DatabaseId, _model.CollectionId);
        }
    }
}
