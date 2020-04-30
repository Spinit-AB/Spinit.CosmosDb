namespace Spinit.CosmosDb.FunctionalTests
{
    internal static class FunctionTestsConfiguration
    {
        public const string CosmosDbConnectionString = "";

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
