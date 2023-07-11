using Xunit;

namespace Spinit.CosmosDb.Tests.Core
{
    public class DatabaseFixture : DatabaseAdapter, IAsyncLifetime
    {
        public Task InitializeAsync() => SetupDatabase();

        public Task DisposeAsync() => TeardownDatabase();
    }
}
