using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spinit.CosmosDb
{
    internal class SkipCosmosEntityIdContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            if (member is PropertyInfo propertyInfo)
            {
                if (typeof(ICosmosEntity).IsAssignableFrom(member.DeclaringType) && member.Name == nameof(ICosmosEntity.Id))
                    return null;
                return base.CreateProperty(member, memberSerialization);
            }
            return null;
        }
    }
}
