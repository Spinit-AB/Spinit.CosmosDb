using System;
using System.Collections.Generic;

namespace Spinit.CosmosDb.UnitTests.Helpers
{
    public class TestEntity : ICosmosEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> All { get; set; }
    }
}
