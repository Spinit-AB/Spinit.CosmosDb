using System;
using System.Linq;
using AutoFixture;
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
                Assert.NotEqual(defaultValue, property?.GetValue(_target));
            }

            [Fact]
            public void QueryShouldBeSet()
            {
                Assert.Equal(Source.Query, _target.Query);
            }

            [Fact]
            public void PageSizeShouldBeSet()
            {
                Assert.Equal(Source.PageSize, _target.PageSize);
            }

            [Fact]
            public void ContinuationTokenShouldBeSet()
            {
                Assert.Equal(Source.ContinuationToken, _target.ContinuationToken);
            }

            [Fact]
            public void FilterShouldBeSet()
            {
                Assert.Equal(Source.Filter, _target.Filter);
            }

            [Fact]
            public void SortByShouldBeSet()
            {
                Assert.Equal(Source.SortBy, _target.SortBy);
            }

            [Fact]
            public void SortDirectionShouldBeSet()
            {
                Assert.Equal(Source.SortDirection, _target.SortDirection);
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
