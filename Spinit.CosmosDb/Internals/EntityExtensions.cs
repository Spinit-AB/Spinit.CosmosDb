using Newtonsoft.Json;

namespace Spinit.CosmosDb
{
    internal static class EntityExtensions
    {
        internal static TEntity CreateNormalized<TEntity>(this TEntity entity, JsonSerializerSettings jsonSerializerSettings)
            where TEntity : ICosmosEntity
        {
            var normalized = JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(entity, jsonSerializerSettings).ToLowerInvariant(), jsonSerializerSettings);
            // restore original id
            normalized.Id = entity.Id;
            return normalized;
        }
    }
}
