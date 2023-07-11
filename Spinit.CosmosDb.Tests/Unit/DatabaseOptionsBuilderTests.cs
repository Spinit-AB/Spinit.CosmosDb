using Shouldly;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit
{
    public class DatabaseOptionsBuilderTests
    {
        public class WhenParsingConnectionString
        {
            private readonly DatabaseOptions<DummyDatabase> _options;
            private const string AccountEndpoint = "https://localhost:8081/";
            private const string AccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            private const string DatabaseId = "MyDatabase";
            private const string PreferredLocation = "West Europe";

            public WhenParsingConnectionString()
            {
                var connectionString = $"AccountEndpoint={AccountEndpoint};AccountKey={AccountKey};DatabaseId={DatabaseId};PreferredLocation={PreferredLocation}";
                _options = new DatabaseOptionsBuilder<DummyDatabase>().UseConnectionString(connectionString).Options;
            }

            [Fact]
            public void AccountEndpointShouldBeParsed()
            {
                _options.Endpoint.ShouldBe(AccountEndpoint);
            }

            [Fact]
            public void AccountKeyShouldBeParsed()
            {
                _options.Key.ShouldBe(AccountKey);
            }

            [Fact]
            public void DatabaseIdShouldBeParsed()
            {
                _options.DatabaseId.ShouldBe(DatabaseId);
            }

            [Fact]
            public void PreferredLocationShouldBeParsed()
            {
                _options.PreferredLocation.ShouldBe(PreferredLocation);
            }
        }

        public class DummyDatabase : CosmosDatabase
        {
            public DummyDatabase(DatabaseOptions<DummyDatabase> options)
                : base(options)
            { }
        }
    }
}
