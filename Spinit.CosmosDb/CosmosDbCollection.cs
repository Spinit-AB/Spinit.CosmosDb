using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Spinit.CosmosDb.ExpressionExtensions;

namespace Spinit.CosmosDb
{
    internal class CosmosDbCollection<TEntity> : ICosmosDbCollection<TEntity>
        where TEntity : class, ICosmosEntity
    {
        private readonly IDocumentClient _documentClient;
        private readonly string _databaseId;
        private readonly string _collectionId;

        public CosmosDbCollection(IDocumentClient documentClient, string databaseId, string collectionId)
        {
            _documentClient = documentClient;
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

        public Task<TEntity> GetAsync(string id) => GetEntityAsync<TEntity>(id);

        public Task<TProjection> GetAsync<TProjection>(string id) where TProjection : class, ICosmosEntity => GetEntityAsync<TProjection>(id);

        private async Task<T> GetEntityAsync<T>(string id)
            where T : class, ICosmosEntity
        {
            var searchResponse = await SearchAsync<T>(new SearchRequest<TEntity>
            {
                Filter = x => x.Id == id
            });

            try
            {
                return searchResponse.Documents.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Multiple documents found with id {id}");
            }
        }


        public Task<SearchResponse<TEntity>> SearchAsync(ISearchRequest<TEntity> request)
        {
            return SearchAsync<TEntity>(request);
        }

        public async Task<SearchResponse<TProjection>> SearchAsync<TProjection>(ISearchRequest<TEntity> request)
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
                var terms = TermAnalyzer.Analyze(request.Query);
                foreach (var term in terms)
                    query = query.Where(x => x.All.Contains(term));
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

            var response = await query.Select(x => x.Original).AsDocumentQuery().ExecuteNextAsync<TProjection>().ConfigureAwait(false);
            return new SearchResponse<TProjection>
            {
                ContinuationToken = response.ResponseContinuation,
                Documents = response.ToArray(),
                TotalCount = request.IncludeTotalCount
                    ? await query.CountAsync().ConfigureAwait(false)
                    : (int?)null
            };
        }

        public Task UpsertAsync(TEntity document)
        {
            try
            {
                //await _documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId), new DocumentCollection { Id = _collectionId, }).ConfigureAwait(false);
                var entry = new DbEntry<TEntity>(document);
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
            throw new NotImplementedException();
        }

        private Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
        }

        private Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }
    }
}
