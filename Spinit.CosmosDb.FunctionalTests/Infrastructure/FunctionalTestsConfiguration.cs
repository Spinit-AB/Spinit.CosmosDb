using System;

namespace Spinit.CosmosDb.FunctionalTests
{
    internal static class FunctionalTestsConfiguration
    {
        private const string DefaultEmulatorConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string EnvironmentVariableName = "SPINIT_COSMOSDB_FUNCTIONALTESTS_CONNECTIONSTRING";

        public static string CosmosDbConnectionString
        {
            get
            {
                string result = Environment.GetEnvironmentVariable(EnvironmentVariableName);
#if DEBUG
                if (string.IsNullOrEmpty(result))
                    result = DefaultEmulatorConnectionString;
#endif
                return result;
            }
        }

        /// <summary>
        /// True if functional test is enabled (only locally in debug mode)
        /// </summary>
        public static bool Enabled => !string.IsNullOrEmpty(CosmosDbConnectionString);
    }
}
