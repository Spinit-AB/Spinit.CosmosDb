﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Spinit.Expressions;

namespace Spinit.CosmosDb
{
    internal class CosmosDbCollection<TEntity> : ICosmosDbCollection<TEntity>
        where TEntity : class, ICosmosEntity
    {
        private readonly IDocumentClient _documentClient;
        private readonly CollectionModel _model;

        public CosmosDbCollection(IDocumentClient documentClient, CollectionModel model)
        {
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
            var response = await _documentClient.ReadDocumentAsync<DbEntry<TProjection>>(GetDocumentUri(id), new RequestOptions { PartitionKey = new PartitionKey(id) }).ConfigureAwait(false);
            return response.Document.Original;
        }

        public Task UpsertAsync(TEntity document)
        {
            try
            {
                var entry = new DbEntry<TEntity>(document, _model.Analyzer);
                return _documentClient.UpsertDocumentAsync(GetCollectionUri(), entry, disableAutomaticIdGeneration: true);
            }
            catch (DocumentClientException)
            {
                // TODO: handle HTTP 429 (Too Many Requests) errors
                throw;
            }
        }

        public Task DeleteAsync(string id)
        {
            return _documentClient.DeleteDocumentAsync(GetDocumentUri(id), new RequestOptions()
            {
                PartitionKey = new PartitionKey(id)
            });
        }

        internal protected virtual async Task<SearchResponse<TProjection>> ExecuteSearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity
        {
            var feedOptions = new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                MaxItemCount = request.PageSize,
                RequestContinuation = request.ContinuationToken
            };

            var query = _documentClient.CreateDocumentQuery<DbEntry<TEntity>>(GetCollectionUri(), feedOptions).AsQueryable();

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

            var feedResponse = await query.Select(x => x.Original).AsDocumentQuery().ExecuteNextAsync<TProjection>().ConfigureAwait(false);

            return new SearchResponse<TProjection>
            {
                ContinuationToken = EncodeContinuationToken(feedResponse.ResponseContinuation),
                Documents = feedResponse.ToArray(),
                TotalCount = request.IncludeTotalCount
                    ? await query.CountAsync().ConfigureAwait(false)
                    : (int?)null
            };
        }

        private Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_model.DatabaseId, _model.CollectionId, id);
        }

        private Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_model.DatabaseId, _model.CollectionId);
        }

        private string EncodeContinuationToken(string continuationToken)
        {
            if (string.IsNullOrEmpty(continuationToken))
                return continuationToken;

            return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(continuationToken));
        }
    }
}