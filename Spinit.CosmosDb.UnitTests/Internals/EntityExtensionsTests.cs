using System;
using AutoFixture;
using Xunit;

namespace Spinit.CosmosDb.UnitTests
{
    public class EntityExtensionsTests
    {
        public class CreateNormalizedTests
        {
            private readonly TestEntity _original;
            private readonly TestEntity _normalized;

            public CreateNormalizedTests()
            {
                var fixture = new Fixture();
                _original = fixture.Create<TestEntity>();
                _normalized = _original.CreateNormalized();
            }

            [Fact]
            public void IdShouldBeEqual()
            {
                Assert.Equal(_original.Id, _normalized.Id);
            }

            [Fact]
            public void StringPropShouldBeLowercased()
            {
                Assert.Equal(_original.StringProp.ToLower(), _normalized.StringProp);
            }

            [Fact]
            public void IntPropShouldBeEqual()
            {
                Assert.Equal(_original.IntProp, _normalized.IntProp);
            }

            [Fact]
            public void BoolPropShouldBeEqual()
            {
                Assert.Equal(_original.BoolProp, _normalized.BoolProp);
            }

            [Fact]
            public void FloatPropShouldBeEqual()
            {
                Assert.Equal(_original.FloatProp, _normalized.FloatProp);
            }

            [Fact]
            public void DateTimePropShouldBeEqual()
            {
                Assert.Equal(_original.DateTimeProp, _normalized.DateTimeProp);
            }

            private class TestEntity : ICosmosEntity
            {
                public string Id { get; set; }
                public string StringProp { get; set; }
                public int IntProp { get; set; }
                public bool BoolProp { get; set; }
                public double FloatProp { get; set; }
                public DateTime DateTimeProp { get; set; }
            }
        }
    }
}
