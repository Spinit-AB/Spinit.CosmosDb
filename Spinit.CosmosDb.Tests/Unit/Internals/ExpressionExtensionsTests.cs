using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Spinit.CosmosDb.Validation;
using Spinit.Expressions;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
{
    /*
    This test is a relic from when ExpressionExtensions.RemapTo was implemented in Spinit.CosmosDb, now it's 
    moved to Spinit.Expressions and tested there.
    But it's keept here since Spinit.CosmosDb is very dependent on this functionality.
    */
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

            [Fact]
            public void TestInterfaceConversion()
            {
                var expression = new Filterer<MyEntity>().Filter();
                var predicate = expression.RemapTo<DbEntry<MyEntity>, MyEntity, bool>(x => x.Original);
                var list = new List<DbEntry<MyEntity>>
                {
                    new DbEntry<MyEntity>(new MyEntity {  CreatedDate = new DateTime(2017, 01, 01) }, new DefaultAnalyzer()),
                    new DbEntry<MyEntity>(new MyEntity {  CreatedDate = new DateTime(2019, 01, 01) }, new DefaultAnalyzer())
                };

                var result = list.AsQueryable().Where(predicate);
                Assert.Contains(result, x => x.Original.CreatedDate.Year == 2017);
                Assert.DoesNotContain(result, x => x.Original.CreatedDate.Year == 2019);
            }

            private class MyEntity : ICosmosEntity, IPartialInterface
            {
                public string? Id { get; set; }
                public DateTime CreatedDate { get; set; }
                public DateTime? ModifiedDate { get; set; }
                public string? MyEntityProp { get; set; }
            }

            public class Filterer<T> where T : IPartialInterface
            {
                public Expression<Func<T, bool>> Filter() => x => x.CreatedDate < new DateTime(2018, 01, 01);
            }

            public interface IPartialInterface
            {
                DateTime CreatedDate { get; }
            }
        }
    }
}
