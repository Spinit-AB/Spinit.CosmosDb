using System.Collections.Generic;
using System.Linq;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// An analyzer is used to generate tokens from an entiy or a search query.
    /// <para>
    /// An analyzer is built from three lower-level building blocks: text extractors, tokenizers, and token filters.
    /// </para>
    /// <see cref="ITextExtractor"/>
    /// <see cref="ITokenizer"/>
    /// <see cref="ITokenFilter"/>
    /// </summary>
    public class Analyzer
    {
        public Analyzer(IEnumerable<ITextExtractor> textExtractors, ITokenizer tokenizer, IEnumerable<ITokenFilter> tokenFilters)
        {
            TextExtractors = textExtractors;
            Tokenizer = tokenizer;
            TokenFilters = tokenFilters;
        }

        internal IEnumerable<ITextExtractor> TextExtractors { get; set; }

        internal ITokenizer Tokenizer { get; set; }

        internal IEnumerable<ITokenFilter> TokenFilters { get; set; }

        /// <summary>
        /// Extracts tokens for an entity
        /// <para>Used when upserting/indexing an entity</para>
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="document">The entity to analyze</param>
        /// <returns></returns>
        public IEnumerable<string> AnalyzeEntity<TEntity>(TEntity document)
            where TEntity : class, ICosmosEntity
        {
            var texts = TextExtractors.ExtractText(document);
            return texts
                .SelectMany(text => Analyze(text, AnalyzeContext.Entity))
                .Distinct();
        }

        /// <summary>
        /// Extracts tokens for a search query
        /// </summary>
        /// <param name="query">The search query to analyze</param>
        /// <returns></returns>
        public IEnumerable<string> AnalyzeQuery(string query)
        {
            return Analyze(query, AnalyzeContext.Query);
        }

        private IEnumerable<string> Analyze(string text, AnalyzeContext analyzeContext)
        {
            var tokens = Tokenizer.Tokenize(text);
            foreach (var tokenFilter in TokenFilters)
            {
                tokens = tokenFilter.Execute(tokens, analyzeContext);
            }
            return tokens;
        }
    }
}
