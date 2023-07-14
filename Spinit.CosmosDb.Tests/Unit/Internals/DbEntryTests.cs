using Shouldly;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
{
    public class DbEntryTests
    {
        public class WhenCreating
        {
            private readonly TestEntity _entity;
            private readonly DbEntry<TestEntity> _dbEntry;

            public WhenCreating()
            {
                _entity = new TestEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test Entity"
                };
                _dbEntry = new DbEntry<TestEntity>(_entity, new DefaultAnalyzer());
            }

            [Fact]
            public void IdShouldBeSet()
            {
                _dbEntry.Id.ShouldNotBeEmpty();
            }

            [Fact]
            public void IdShouldBeSetFromSource()
            {
                _dbEntry.Id.ShouldBe(_entity.Id);
            }

            [Fact]
            public void OriginalShouldBeSet()
            {
                _dbEntry.Original.ShouldNotBeNull();
            }

            [Fact]
            public void OriginalShouldBeEqualToSourceEntity()
            {
                _dbEntry.Original.ShouldBe(_entity);
            }

            [Fact]
            public void NormalizedShouldBeSet()
            {
                _dbEntry.Normalized.ShouldNotBeNull();
            }

            [Fact]
            public void NormalizedNameShouldBeLowercase()
            {
                _dbEntry.Normalized.Name.ShouldBe("test entity");
            }

            [Fact]
            public void AllFieldShouldBeSet()
            {
                _dbEntry.All.ShouldNotBeNull();
            }

            [Fact]
            public void AllFieldShouldNonEmpty()
            {
                _dbEntry.All.ShouldNotBeEmpty();
            }

            public class TestEntity : ICosmosEntity
            {
                public required string Id { get; set; }
                public string? Name { get; set; }
            }
        }
    }
}
