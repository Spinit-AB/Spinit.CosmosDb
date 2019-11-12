using System;

namespace Spinit.CosmosDb.Analysis
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SearchableAttribute : Attribute
    {
        public SearchableAttribute(bool isSearchable = true)
        {
            IsSearchable = isSearchable;
        }

        public bool IsSearchable { get; }
    }
}
