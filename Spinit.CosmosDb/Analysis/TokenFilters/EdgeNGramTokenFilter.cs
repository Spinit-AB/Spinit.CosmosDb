using System;
using System.Collections.Generic;
using System.Linq;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Emits N-grams of each word where the start of the N-gram is anchored to the beginning of the word.
    /// <para>
    /// Edge N-Grams are useful for search-as-you-type queries.
    /// </para>
    /// </summary>
    public class EdgeNGramTokenFilter : ITokenFilter
    {
        public IEnumerable<string> Execute(IEnumerable<string> tokens, AnalyzeContext context)
        {
            switch (context)
            {
                case AnalyzeContext.Entity:
                    return tokens.SelectMany(GenerateEdgeNGram);
                case AnalyzeContext.Query:
                    return tokens;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }
        }

        internal static IEnumerable<string> GenerateEdgeNGram(string token)
        {
            for (int i = 1; i < token.Length; i++)
            {
                yield return token.Substring(0, i);
            }
            yield return token;
        }
    }
}
