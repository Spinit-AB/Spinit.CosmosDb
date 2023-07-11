using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb
{
    public interface ICosmosDbCollection<TEntity>
        where TEntity : class, ICosmosEntity
    {
        /// <summary>
        /// Searches for entities
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SearchResponse<TEntity>> SearchAsync(ISearchRequest<TEntity> request);
        Task<SearchResponse<TProjection>> SearchAsync<TProjection>(ISearchRequest<TEntity> request)
            where TProjection : class, ICosmosEntity;

        /// <summary>
        /// Returns a single entity by id
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(string id);
        Task<TProjection> GetAsync<TProjection>(string id)
            where TProjection : class, ICosmosEntity;

        /// <summary>
        /// Bulk imports and/or updates a list of entities.
        /// </summary>
        Task UpsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds or updates an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpsertAsync(TEntity entity);

        // TODO: Only for C# 8
        //Task DeleteAsync(T document) => DeleteAsync(document.Id);

        /// <summary>
        /// Deletes an entity by id
        /// </summary>
        /// <param name="id">The id of the entity to delete</param>
        /// <returns></returns>
        Task DeleteAsync(string id);

        /// <summary>
        /// Deletes a list of entities.
        /// </summary>
        Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the throughput (RU/s) set for the collection.
        /// </summary>
        /// <returns>The collection's throughput</returns>
        Task<ThroughputProperties> GetThroughputAsync();

        /// <summary>
        /// Sets the throughput (RU/s) for the collection.
        /// </summary>
        /// <param name="throughputProperties">The new throughput to set. Must be between 400 and 1000000 in increments of 100.</param>
        /// <returns></returns>
        Task SetThroughputAsync(ThroughputProperties throughputProperties);

        /// <summary>
        /// Gets the number of entities.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<int> CountAsync(ISearchRequest<TEntity> request);
    }
}
