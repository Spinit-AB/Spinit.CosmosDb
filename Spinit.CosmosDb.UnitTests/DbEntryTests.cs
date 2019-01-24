using System;
using Spinit.CosmosDb;
using Xunit;

namespace Spinit.CosmosDb.UnitTests
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
                    Id = Guid.NewGuid(),
                    Name = "Test Entity"
                };
                _dbEntry = new DbEntry<TestEntity>(_entity);
            }

            [Fact]
            public void IdShouldBeSet()
            {
                Assert.NotEqual(Guid.Empty, _dbEntry.Id);
            }

            [Fact]
            public void IdShouldBeSetFromSource()
            {
                Assert.Equal(_entity.Id, _dbEntry.Id);
            }

            [Fact]
            public void OriginalShouldBeSet()
            {
                Assert.NotNull(_dbEntry.Original);
            }

            [Fact]
            public void OriginalShouldBeEqualToSourceEntity()
            {
                Assert.Equal(_entity, _dbEntry.Original);
            }

            [Fact]
            public void NormalizedShouldBeSet()
            {
                Assert.NotNull(_dbEntry.Normalized);
            }

            [Fact]
            public void NormalizedNameShouldBeLowecase()
            {
                Assert.Equal("test entity", _dbEntry.Normalized.Name);
            }

            [Fact]
            public void AllFieldShouldBeSet()
            {
                Assert.NotNull(_dbEntry.All);
            }

            [Fact]
            public void AllFieldShouldNonEmpty()
            {
                Assert.NotEmpty(_dbEntry.All);
            }

            public class TestEntity : ICosmosEntity
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
