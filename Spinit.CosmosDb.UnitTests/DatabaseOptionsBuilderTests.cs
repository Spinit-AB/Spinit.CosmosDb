using Xunit;

namespace Spinit.CosmosDb.UnitTests
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
                Assert.Equal(AccountEndpoint, _options.Endpoint);
            }

            [Fact]
            public void AccountKeyShouldBeParsed()
            {
                Assert.Equal(AccountKey, _options.Key);
            }

            [Fact]
            public void DatabaseIdShouldBeParsed()
            {
                Assert.Equal(DatabaseId, _options.DatabaseId);
            }

            [Fact]
            public void PreferredLocationShouldBeParsed()
            {
                Assert.Equal(PreferredLocation, _options.PreferredLocation);
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
