using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    public interface ICosmosDbCollection<T>
        where T : class, ICosmosEntity
    {
        Task<T> GetAsync(string id);
        Task<TProjection> GetAsync<TProjection>(string id)
            where TProjection : class, ICosmosEntity;
        Task UpsertAsync(T document);

        // TODO: Only for C# 8
        //Task DeleteAsync(T document) => DeleteAsync(document.Id);
        Task DeleteAsync(string id);
        Task<SearchResponse<T>> SearchAsync(ISearchRequest<T> request);
        Task<SearchResponse<TProjection>> SearchAsync<TProjection>(ISearchRequest<T> request)
            where TProjection : class, ICosmosEntity;
    }
}
