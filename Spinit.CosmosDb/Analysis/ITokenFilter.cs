using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Token filters accept a stream of tokens from a tokenizer and can modify tokens (eg lowercasing), delete tokens (eg remove stopwords) or add tokens (eg synonyms).
    /// </summary>
    public interface ITokenFilter
    {
        IEnumerable<string> Execute(IEnumerable<string> tokens, AnalyzeContext context);
    }
}
