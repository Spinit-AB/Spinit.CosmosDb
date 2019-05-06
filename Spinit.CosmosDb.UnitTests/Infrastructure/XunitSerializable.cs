using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Spinit.CosmosDb.UnitTests.Infrastructure
{
    public class XunitSerializable : IXunitSerializable
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public void Deserialize(IXunitSerializationInfo info)
        {
            JsonConvert.PopulateObject(info.GetValue<string>("json"), this, settings);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("json", JsonConvert.SerializeObject(this, settings));
        }
    }
}
