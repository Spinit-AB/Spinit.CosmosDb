using System.Collections.Generic;
using System.Linq;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Base interface for a text extractor, that is used to extract string data from an entity.
    /// </summary>
    public interface ITextExtractor
    {
        /// <summary>
        /// Extracts texts from an entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entity">The entity to extract texts from</param>
        /// <returns></returns>
        IEnumerable<string> ExtractText<TEntity>(TEntity entity)
            where TEntity : ICosmosEntity;
    }

    public static class TextExtractorExtensions
    {
        /// <summary>
        /// Extracts texts from multiple <see cref="ITextExtractor"/>'s
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="extractors"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<string> ExtractText<TEntity>(this IEnumerable<ITextExtractor> extractors, TEntity entity)
            where TEntity : ICosmosEntity
        {
            if (!extractors.Any())
                return null;
            return extractors
                .SelectMany(extractor => extractor.ExtractText(entity))
                .Distinct();
        }
    }
}
