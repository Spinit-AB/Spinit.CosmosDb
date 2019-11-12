using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Spinit.CosmosDb.Analysis;

namespace Spinit.CosmosDb
{
    internal class ContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            if (member is PropertyInfo propertyInfo)
            {
                var searchable = member.GetCustomAttribute<SearchableAttribute>();
                if (searchable != null)
                {
                    if (searchable.IsSearchable)
                    {
                        return base.CreateProperty(member, memberSerialization);
                    }

                    return null;
                }

                if (typeof(ICosmosEntity).IsAssignableFrom(member.DeclaringType) && member.Name == nameof(ICosmosEntity.Id))
                    return null;

                if (propertyInfo.PropertyType.IsValueType && propertyInfo.PropertyType != typeof(DateTime) && propertyInfo.PropertyType != typeof(DateTime?))
                {
                    return null;
                }

                return base.CreateProperty(member, memberSerialization);
            }

            return null;
        }
    }
}
