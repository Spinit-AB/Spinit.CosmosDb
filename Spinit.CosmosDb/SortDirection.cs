using System.ComponentModel;

namespace Spinit.CosmosDb
{
    public enum SortDirection
    {
        [Description("Ascending")]
        Ascending = 0,
        [Description("Descending")]
        Descending = 1,
    }
}
