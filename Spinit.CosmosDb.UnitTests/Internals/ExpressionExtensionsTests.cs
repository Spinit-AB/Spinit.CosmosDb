using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Spinit.CosmosDb.UnitTests
{
    public class ExpressionExtensionsTests
    {
        public class RemapToTests
        {
            [Fact]
            public void ToStringShouldMatchExpectation()
            {
                Expression<Func<MyEntity, DateTime>> expression = x => x.CreatedDate;
                Expression<Func<DbEntry<MyEntity>, DateTime>> expectedResult = x => x.Normalized.CreatedDate;
                var result = expression.RemapTo<DbEntry<MyEntity>, MyEntity, DateTime>(x => x.Normalized);
                Assert.Equal(expectedResult.ToString(), result.ToString());
            }

            [Fact]
            public void DocumentationExampleShouldWork()
            {
                Expression<Func<MyEntity, bool>> source = x => x.MyEntityProp == "SomeValue";
                var remappedExpression = source.RemapTo<DbEntry<MyEntity>, MyEntity, bool>(x => x.Original);

                var list = new List<DbEntry<MyEntity>>
                {
                    new DbEntry<MyEntity>(new MyEntity{ MyEntityProp = "SomeValue"}, new DefaultAnalyzer())
                };

                Assert.Single(list.AsQueryable().Where(remappedExpression));
            }

            [Fact]
            public void UsingGetValueOrDefaultOnNullableTypesShouldWork()
            {
                Expression<Func<MyEntity, bool>> expression = x => x.ModifiedDate.GetValueOrDefault(DateTime.MinValue) >= DateTime.UtcNow;
                var result = expression.RemapTo<DbEntry<MyEntity>, MyEntity, bool>(x => x.Original);
            }

            [Fact]
            public void UsingGetValueOrDefaultOnNullableTypesShouldReturnExpectedResult()
            {
                var now = DateTime.UtcNow;
                Expression<Func<MyEntity, bool>> expression = x => x.ModifiedDate.GetValueOrDefault(DateTime.MinValue) >= now.AddDays(-7);
                var result = expression.RemapTo<DbEntry<MyEntity>, MyEntity, bool>(x => x.Original);
                var list = new List<DbEntry<MyEntity>>
                {
                    new DbEntry<MyEntity>(new MyEntity{ Id = "ShouldBeFound", ModifiedDate = now.AddDays(-1) }, new DefaultAnalyzer()),
                    new DbEntry<MyEntity>(new MyEntity{ Id = "ShouldNotBeFound", ModifiedDate = null }, new DefaultAnalyzer()),
                };
                Assert.Contains(list.AsQueryable().Where(result), x => x.Id == "ShouldBeFound");
                Assert.DoesNotContain(list.AsQueryable().Where(result), x => x.Id == "ShouldNotBeFound");
            }

            private class MyEntity : ICosmosEntity
            {
                public string Id { get; set; }
                public DateTime CreatedDate { get; set; }
                public DateTime? ModifiedDate { get; set; }
                public string MyEntityProp { get; set; }
            }
        }
    }
}
