using System;
using System.Collections.Generic;
using Spinit.CosmosDb;

namespace TodoApi.Features.Shared
{
    public class TodoItem : ICosmosEntity
    {
        public string Id { get; set; }
        public TodoStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
