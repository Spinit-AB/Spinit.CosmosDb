using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// The default analyzer.
    /// <para>
    /// Extracts only string properties, splits tokens on non-word characters ans lowercases all tokens.
    /// </para>
    /// </summary>
    public class DefaultAnalyzer : Analyzer
    {
        public DefaultAnalyzer()
            : base(CreateDefaultTextExtractors(), new PatternTokenizer(), new List<ITokenFilter> { new LowercaseTokenFilter() })
        {
        }

        private static IEnumerable<ITextExtractor> CreateDefaultTextExtractors()
        {
            yield return new StringPropertyTextExtractor();
        }
    }
}
