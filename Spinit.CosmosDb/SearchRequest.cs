using System;
using System.Linq.Expressions;

namespace Spinit.CosmosDb
{
    public interface ISearchRequest<T>
        where T : class
    {
        string Query { get; }

        int? PageSize { get; }
        string ContinuationToken { get; }

        Expression<Func<T, bool>> Filter { get; }

        Expression<Func<T, object>> SortBy { get; }
        SortDirection SortDirection { get; }
    }

    public class SearchRequest<T> : ISearchRequest<T>
        where T : class
    {
        public virtual string Query { get; set; }

        public virtual int? PageSize { get; set; } = 20;
        public virtual string ContinuationToken { get; set; }

        public virtual Expression<Func<T, bool>> Filter { get; set; }

        public virtual Expression<Func<T, object>> SortBy { get; set; }
        public virtual SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    public static class SearchRequestExtensions
    {
        public static SearchRequest<T> Assign<T>(this SearchRequest<T> target, ISearchRequest<T> source)
            where T : class
        {
            target.Query = source.Query;
            target.PageSize = source.PageSize;
            target.ContinuationToken = source.ContinuationToken;
            target.Filter = source.Filter;
            target.SortBy = source.SortBy;
            target.SortDirection = source.SortDirection;
            return target;
        }
    }
}
