using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    public class SearchResponse<T>
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<T> Documents { get; set; }
        /// <summary>
        /// Total record count, only calculated if <see cref="ISearchRequest{T}.IncludeTotalCount"/> is set
        /// </summary>
        public int? TotalCount { get; set; }
    }
}
