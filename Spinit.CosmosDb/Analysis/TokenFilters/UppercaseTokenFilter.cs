using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Normalizes token text to upper case.
    /// </summary>
    public class UppercaseTokenFilter : ITokenFilter
    {
        public IEnumerable<string> Execute(IEnumerable<string> tokens, AnalyzeContext context)
        {
            foreach (var token in tokens)
            {
                yield return token.ToUpperInvariant();
            }
        }
    }
}
