using System;
using System.Collections.Generic;
using System.Linq;
using Spinit.CosmosDb.Analysis;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Analysis.TextExtractors
{
    public class DefaultAnalyzerPropertyTextExtractorTests
    {
        [Fact]
        public void ExtractTextShouldReturnExpectedValue()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product();
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTextShouldIncludeStringsInNestedObjects()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product
            {
                Description = "Test1",
                SimilarProducts = new[]
                {
                    new Product
                    {
                        Description = "Test2"
                    }
                }
            };
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { "test1", "test2" };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTextShouldIncludeSearchable()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product
            {
                Price = 10,
                Stock = 5,
            };
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { "5" };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTextShouldExcludeNotSearchable()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product
            {
                Title = "TestTitle",
                Description = "TestDescription"
            };
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { "testdescription" };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTextShouldIncludePrimitivesWithSearchable()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product2
            {
                Test1 = 1,
                Test1a = 10,
                Test2 = 2,
                Test2a = 20,
                Test3 = 3,
                Test3a = 30,
                Test4 = 4,
                Test4a = 40,
                Test5 = 5,
                Test5a = 50,
                Test6 = true,
                Test6a = false
            };
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { "1", "10", "2", "20", "3", "30", "4", "40", "5", "50", "true", "false" };
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTextShouldIncludePrimitivesWithSearchableInNestedObjects()
        {
            var analyzer = new DefaultAnalyzer();
            var entity = new Product
            {
                Stock = 3,
                RelatedProduct = new Product
                {
                    Stock = 4
                }
            };
            var result = analyzer.AnalyzeEntity(entity).ToArray();
            string[] expected = new string[] { "3", "4" };
            Assert.Equal(expected, result);
        }

        public class Product : ICosmosEntity
        {
            public string Id { get; set; }

            [Searchable(false)]
            public string Title { get; set; }

            public string Description { get; set; }

            public double Price { get; set; }

            [Searchable]
            public int? Stock { get; set; }

            public bool OutOfStock { get; set; }

            public IEnumerable<string> Tags { get; set; }

            public IEnumerable<Product> SimilarProducts { get; set; }

            public Product RelatedProduct { get; set; }
        }

        public class Product2 : ICosmosEntity
        {
            public string Id { get; set; }

            [Searchable]
            public short Test1 { get; set; }

            [Searchable]
            public short? Test1a { get; set; }

            [Searchable]
            public int Test2 { get; set; }

            [Searchable]
            public int? Test2a { get; set; }

            [Searchable]
            public long Test3 { get; set; }

            [Searchable]
            public long? Test3a { get; set; }

            [Searchable]
            public double Test4 { get; set; }

            [Searchable]
            public double? Test4a { get; set; }

            [Searchable]
            public decimal Test5 { get; set; }

            [Searchable]
            public decimal? Test5a { get; set; }

            [Searchable]
            public bool Test6 { get; set; }

            [Searchable]
            public bool? Test6a { get; set; }
        }
    }
}
