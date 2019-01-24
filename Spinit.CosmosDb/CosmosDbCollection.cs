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

        public async Task<TEntity> GetAsync(Guid id)
        {
            var options = new RequestOptions { PartitionKey = new PartitionKey(id.ToString()) };
            var result = await _documentClient.ReadDocumentAsync<DbEntry<TEntity>>(GetDocumentUri(id), options).ConfigureAwait(false);
            return result.Document.Original;
        }

        public async Task<SearchResponse<TEntity>> SearchAsync(ISearchRequest<TEntity> request)
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

            try
            {
                var response = await query.Select(x => x.Original).AsDocumentQuery().ExecuteNextAsync<TEntity>().ConfigureAwait(false);
                return new SearchResponse<TEntity>
                {
                    ContinuationToken = response.ResponseContinuation,
                    Documents = response.ToArray()
                };
            }
            catch (Exception e)
            {
                throw e;
            }
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

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        private Uri GetDocumentUri(Guid id)
        {
            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id.ToString());
        }

        private Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }
    }
}
