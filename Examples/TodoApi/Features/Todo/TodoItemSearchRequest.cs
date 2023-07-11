using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TodoApi.Features.Shared;
using static TodoApi.Features.Todo.TodoItemSearchRequest;

namespace TodoApi.Features.Todo
{
    public record TodoItemSearchRequest(string? Query = null, string? ContinuationToken = null, SearchFilter? Filter = null)
    {        
        public record SearchFilter(IReadOnlyList<TodoStatus>? Status = null, Range<DateTime>? CreatedDate = null, IReadOnlyList<string>? Tags = null)
        {
            internal Expression<Func<TodoItem, bool>>? AsExpression()
            {
                var filters = new List<Expression<Func<TodoItem, bool>>>();

                if (Status is { Count: > 0 })
                {
                    filters.Add(x => Status.Contains(x.Status));
                }

                if (CreatedDate != null && !CreatedDate.IsEmpty())
                {
                    filters.Add(x =>
                        x.CreatedDate >= CreatedDate.Min.GetValueOrDefault(DateTime.MinValue) &&
                        x.CreatedDate <= CreatedDate.Max.GetValueOrDefault(DateTime.MaxValue));
                }

                if (Tags is { Count: > 0 })
                {
                    filters.Add(x => Tags.Any(t => x.Tags.Contains(t)));
                }

                if (!filters.Any())
                {
                    return null;
                }


                Expression<Func<TodoItem, bool>>? result = null;
                foreach (var filter in filters)
                {
                    if (result == null)
                    {
                        result = filter;
                    }
                    else
                    {
                        result = result.And(filter);
                    }

                }

                return result;
            }
        }
    }    
}
