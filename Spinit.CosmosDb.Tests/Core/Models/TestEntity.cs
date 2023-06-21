using System.Collections.Generic;

namespace Spinit.CosmosDb.Tests.Core.Models
{
    public class TestEntity : ICosmosEntity
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public required IEnumerable<string> All { get; set; }
    }
}
