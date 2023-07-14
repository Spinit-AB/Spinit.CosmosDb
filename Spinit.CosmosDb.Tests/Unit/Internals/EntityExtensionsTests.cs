using AutoFixture;
using Shouldly;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
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
                _normalized = _original.CreateNormalized(new Newtonsoft.Json.JsonSerializerSettings());
            }

            [Fact]
            public void IdShouldBeEqual()
            {
                _normalized.Id.ShouldBe(_original.Id);
            }

            [Fact]
            public void StringPropShouldBeLowercased()
            {
                _normalized.StringProp.ShouldBe(_original.StringProp?.ToLower());
            }

            [Fact]
            public void IntPropShouldBeEqual()
            {
                _normalized.IntProp.ShouldBe(_original.IntProp);
            }

            [Fact]
            public void BoolPropShouldBeEqual()
            {
                _normalized.BoolProp.ShouldBe(_original.BoolProp);
            }

            [Fact]
            public void FloatPropShouldBeEqual()
            {
                _normalized.FloatProp.ShouldBe(_original.FloatProp);
            }

            [Fact]
            public void DateTimePropShouldBeEqual()
            {
                _normalized.DateTimeProp.ShouldBe(_original.DateTimeProp);
            }

            private class TestEntity : ICosmosEntity
            {
                public required string Id { get; set; }
                public string? StringProp { get; set; }
                public int IntProp { get; set; }
                public bool BoolProp { get; set; }
                public double FloatProp { get; set; }
                public DateTime DateTimeProp { get; set; }
            }
        }
    }
}
