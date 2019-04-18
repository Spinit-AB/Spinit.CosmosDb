namespace Spinit.CosmosDb.FunctionalTests
{
    internal static class FunctionTestsConfiguration
    {
        public const string CosmosDbConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public const string SkipTests =
#if DEBUG
            null;
#else
            "Functional test should only run locally in debug mode";
#endif

        /// <summary>
        /// True if functional test is enabled (only locally in debug mode)
        /// </summary>
        public static bool Enabled => string.IsNullOrEmpty(SkipTests);
    }
}
