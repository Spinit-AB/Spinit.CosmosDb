using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    public class SearchResponse<T>
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<T> Documents { get; set; }
    }
}
