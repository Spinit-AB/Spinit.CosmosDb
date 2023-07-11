using AutoFixture;
using Shouldly;
using Spinit.CosmosDb.Tests.Core.Models;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit
{
    public class SearchRequestTests
    {
        public class Assign : AssignTest<SearchRequest<TestEntity>>
        {
            private SearchRequest<TestEntity> _target { get; }

            public Assign()
            {
                _target = new SearchRequest<TestEntity>().Assign(Source);
            }

            [Theory]
            [MemberData(nameof(TypeExtensions.GetPropertyNames), typeof(SearchRequest<TestEntity>), MemberType = typeof(TypeExtensions))]
            public void AllPropertiesShouldBeSet(string propertyName)
            {
                var property = _target.GetType().GetProperty(propertyName);
                var defaultValue = property?.PropertyType.GetDefaultValue();
                property?.GetValue(_target).ShouldNotBe(defaultValue);
            }

            [Fact]
            public void QueryShouldBeSet()
            {
                _target.Query.ShouldBe(Source.Query);
            }

            [Fact]
            public void PageSizeShouldBeSet()
            {
                _target.PageSize.ShouldBe(Source.PageSize);
            }

            [Fact]
            public void ContinuationTokenShouldBeSet()
            {
                _target.ContinuationToken.ShouldBe(Source.ContinuationToken);
            }

            [Fact]
            public void FilterShouldBeSet()
            {
                _target.Filter.ShouldBe(Source.Filter);
            }

            [Fact]
            public void SortByShouldBeSet()
            {
                _target.SortBy.ShouldBe(Source.SortBy);
            }

            [Fact]
            public void SortDirectionShouldBeSet()
            {
                _target.SortDirection.ShouldBe(Source.SortDirection);
            }

            protected override void SetupFixture()
            {
                Fixture.Customize<SortDirection>(c => c.FromFactory(() => SortDirection.Descending));
            }
        }
    }

    public abstract class AssignTest<T>
    {
        protected Fixture Fixture { get; }
        protected T Source { get; }

        public AssignTest()
        {
            Fixture = new Fixture();
            SetupFixture();
            Source = Fixture.Create<T>();
        }

        protected virtual void SetupFixture()
        {
        }
    }

    public static class TypeExtensions
    {
        public static object? GetDefaultValue(this Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public static TheoryData<string> GetPropertyNames(Type type)
        {
            var result = new TheoryData<string>();
            foreach (var propertyName in type.GetMembers().Where(x => x.MemberType == System.Reflection.MemberTypes.Property).Select(x => x.Name))
            {
                result.Add(propertyName);
            }
            return result;
        }
    }
}
