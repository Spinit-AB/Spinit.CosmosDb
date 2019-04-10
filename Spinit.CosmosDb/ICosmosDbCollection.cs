﻿using System.Threading.Tasks;

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
        
    }
}
