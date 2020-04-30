namespace Spinit.CosmosDb.FunctionalTests
{
    internal static class FunctionTestsConfiguration
    {
        public const string CosmosDbConnectionString = "AccountEndpoint=https://internal-test-database.documents.azure.com:443/;AccountKey=Z9ScXRv42dvqCxlOXN03wFtpr8EI70LfkBmPaTtKjUCgl5RivZJmg1SNh0j7ICroydIclsg18vjAkJ1sVljVnA==;";

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
