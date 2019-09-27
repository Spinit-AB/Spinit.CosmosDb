using System;
using System.Threading.Tasks;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests.Infrastructure
{
    /// <summary>
    /// Fixture for functional tests of a <see cref="CosmosDatabase"/>
    /// <para>
    /// OBSERVE: the <typeparamref name="TDatabase"/> must have the default constructor: <see cref="CosmosDatabase.CosmosDatabase(IDatabaseOptions)"/>
    /// </para>
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    public class CosmosDbFixture<TDatabase> : IAsyncLifetime
        where TDatabase : CosmosDatabase
    {
        public CosmosDbFixture()
        {
            if (FunctionalTestsConfiguration.Enabled)
                Database = (TDatabase)Activator.CreateInstance(typeof(TDatabase), new DatabaseOptionsBuilder<TDatabase>().UseConnectionString(GenerateConnectionString()).Options);
        }

        public TDatabase Database { get; }

        public Task InitializeAsync()
        {
            if (!FunctionalTestsConfiguration.Enabled)
                throw new Exception("Functional tests not enabled (check connection string)");
            return Database.Operations.CreateIfNotExistsAsync();
        }

        public async Task DisposeAsync()
        {
            if (FunctionalTestsConfiguration.Enabled)
                await Database.Operations.DeleteAsync();
        }

        private static string GenerateConnectionString()
        {
            var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
            return $"{FunctionalTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
        }
    }
}
