using System;
using System.Collections.Generic;
using System.Linq;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Builder class for constructing an analyzer.
    /// </summary>
    public class AnalyzerBuilder
    {
        public AnalyzerBuilder()
        {
            TextExtractors = new List<ITextExtractor>();
            Tokenizer = null;
            TokenFilters = new List<ITokenFilter>();
        }

        internal AnalyzerBuilder(Analyzer analyzer)
        {
            TextExtractors = analyzer.TextExtractors.ToList();
            Tokenizer = analyzer.Tokenizer;
            TokenFilters = analyzer.TokenFilters.ToList();
        }

        public ICollection<ITextExtractor> TextExtractors { get; } // TODO replace with IFluentCollection and remove AnalyzerBuilderExtensions
        public ITokenizer Tokenizer { get; }
        public ICollection<ITokenFilter> TokenFilters { get; } // TODO replace with IFluentCollection and remove AnalyzerBuilderExtensions

        public Analyzer Build()
        {
            return new Analyzer(TextExtractors, Tokenizer, TokenFilters);
        }
    }

    public static class AnalyzerBuilderExtensions
    {
        public static ICollection<ITokenFilter> Add<TFilter>(this ICollection<ITokenFilter> tokenFilters)
            where TFilter : class, ITokenFilter, new()
        {
            var filter = (ITokenFilter)Activator.CreateInstance<TFilter>();
            tokenFilters.Add(filter);
            return tokenFilters;
        }

        public static ICollection<ITokenFilter> Add<TFilter>(this ICollection<ITokenFilter> tokenFilters, TFilter filter)
            where TFilter : class, ITokenFilter
        {
            tokenFilters.Add(filter);
            return tokenFilters;
        }
    }
}
