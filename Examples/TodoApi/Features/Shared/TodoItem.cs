using System;
using System.Collections.Generic;
using Spinit.CosmosDb;

namespace TodoApi.Features.Shared
{
    public class TodoItem : ICosmosEntity
    {
        public required string Id { get; set; }
        public required TodoStatus Status { get; set; }
        public required DateTime CreatedDate { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public required IReadOnlyList<string> Tags { get; set; }
    }
}
