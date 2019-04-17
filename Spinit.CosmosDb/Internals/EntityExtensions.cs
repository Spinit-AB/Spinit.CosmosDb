using Newtonsoft.Json;

namespace Spinit.CosmosDb
{
    internal static class EntityExtensions
    {
        internal static TEntity CreateNormalized<TEntity>(this TEntity entity)
            where TEntity : ICosmosEntity
        {
            var normalized = JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(entity).ToLowerInvariant());
            // restore original id
            normalized.Id = entity.Id;
            return normalized;
        }
    }
}
