﻿using System.Collections.Generic;
using System.Linq;
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

        internal static IEnumerable<string> GetAllTextFieldValues(this ICosmosEntity entity)
        {
            var stringProperties = entity.GetType().GetProperties().Where(x => x.PropertyType == typeof(string) && x.Name != nameof(ICosmosEntity.Id));
            return stringProperties.Select(x => x.GetValue(entity)?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }
    }
}
