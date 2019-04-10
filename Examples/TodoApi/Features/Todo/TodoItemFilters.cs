﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TodoApi.Features.Shared;

namespace TodoApi.Features.Todo
{
    public class TodoItemFilters
    {
        public IEnumerable<TodoStatus> Status { get; set; }
        public Range<DateTime> CreatedDate { get; set; }
        public IEnumerable<string> Tags { get; set; }

        internal Expression<Func<TodoItem, bool>> AsExpression()
        {
            var filters = new List<Expression<Func<TodoItem, bool>>>();

            if (Status != null && Status.Any())
            {
                filters.Add(x => Status.Contains(x.Status));
            }

            if (CreatedDate != null && !CreatedDate.IsEmpty())
            {
                filters.Add(x =>
                    x.CreatedDate >= CreatedDate.Min.GetValueOrDefault(DateTime.MinValue) &&
                    x.CreatedDate <= CreatedDate.Max.GetValueOrDefault(DateTime.MaxValue));
            }

            if (Tags != null && Tags.Any())
            {
                filters.Add(x => Tags.Any(t => x.Tags.Contains(t)));
            }

            if (!filters.Any())
                return null;

            Expression<Func<TodoItem, bool>> result = null;
            foreach (var filter in filters)
            {
                if (result == null)
                    result = filter;
                else
                    result = result.And(filter);
            }
            return result;
        }
    }    
}
