using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Normalizes token text to lower case.
    /// </summary>
    public class LowercaseTokenFilter : ITokenFilter
    {
        public IEnumerable<string> Execute(IEnumerable<string> tokens, AnalyzeContext context)
        {
            foreach (var token in tokens)
            {
                yield return token.ToLowerInvariant();
            }
        }
    }
}
