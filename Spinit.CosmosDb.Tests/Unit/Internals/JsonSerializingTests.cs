using System.Reflection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
{
    public class JsonSerializingTests
    {
        private CustomOptions _options;

        public JsonSerializingTests()
        {
            _options = new CustomOptions
            {
                ConfigureJsonSerializerSettings = (settings) =>
                {
                    settings.Converters.Add(new VersionJsonConverter());
                }
            };
        }


        [Fact]
        public void Serialize_custom()
        {
            var obj = new TestEntity(Type: "MyObject", Version: Version.Second);
            var jsonValue = JsonConvert.SerializeObject(obj, CosmosDatabase.CreateJsonSerializerSettings(_options));
            var jObj = JObject.Parse(jsonValue);
            var version = jObj["Version"]?.ToObject<string>();
            version.ShouldBe("v2");
        }

        [Fact]
        public void Deserialize_custom()
        {
            var entity = JsonConvert.DeserializeObject<TestEntity>(
                "{ \"type\": \"MyObject\", \"version\": \"v3\" }",
                CosmosDatabase.CreateJsonSerializerSettings(_options));

            entity?.Version.ShouldBe(Version.Third);
        }

        private record TestEntity(string Type, Version Version);

        private class CustomOptions : IDatabaseOptions
        {
            public string? Endpoint { get; set; }
            public string? Key { get; set; }
            public string? DatabaseId { get; set; }
            public string? PreferredLocation { get; set; }
            public Action<JsonSerializerSettings>? ConfigureJsonSerializerSettings { get; set; }
            public Action<(CosmosClientOptions, bool)>? ConfigureCosmosClientOptions { get; set; }
        }

        public class VersionJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(Version);

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (reader.Value is string id)
                {
                    return Version.GetAll().Single(x => x.Id == id);
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is Version version)
                {
                    writer.WriteValue(version.Id);
                }
            }
        }

        public class Version
        {
            public static readonly Version First = new Version("v1", "First Version", 1);
            public static readonly Version Second = new Version("v2", "Second Version", 2);
            public static readonly Version Third = new Version("v3", "Third Version", 3);

            private Version(string id, string name, int number)
            {
                Id = id;
                Name = name;
                Number = number;
            }

            public string Id { get; }
            public string Name { get; }
            public int Number { get; }

            public static IEnumerable<Version> GetAll()
            {
                var fields = typeof(Version).GetTypeInfo().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                return fields.Select(info => info.GetValue(null)).OfType<Version>();
            }
        }

    }
}
