using System;
using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    public interface ICosmosDbCollection<T>
        where T : class, ICosmosEntity
    {
        Task<T> GetAsync(Guid id);
        Task UpsertAsync(T document);

        // TODO: Only for C# 8
        //Task DeleteAsync(T document) => DeleteAsync(document.Id);
        Task DeleteAsync(Guid id);
        Task<SearchResponse<T>> SearchAsync(ISearchRequest<T> request);
    }
}
