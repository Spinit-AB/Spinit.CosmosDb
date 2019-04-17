using System.Collections.Generic;
using Spinit.CosmosDb.UnitTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Analysis.TextExtractors
{
    public class StringPropertyTextExtractorTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExtractTextShouldReturnExpectedValue(Scenario scenario)
        {
            var extractor = new StringPropertyTextExtractor();
            var result = extractor.ExtractText(scenario.Entity);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Entity = new Product
                    {
                        Id = "Id",
                        Title = "Title",
                        Description = "A long description",
                        Price = 1234,
                        Stock = 56,
                        OutOfStock = false
                    },
                    ExpectedResult = new[] { "Title", "A long description" }
                },
                new Scenario
                {
                    Entity = new Product
                    {
                        Id = "Id",
                        Title = "Title",
                        Description = "A long description",
                        Price = 1234,
                        Stock = 0,
                        OutOfStock = true,
                        Tags = new[] { "Tag1", "Tag2" }
                    },
                    ExpectedResult = new[] { "Title", "A long description", "Tag1", "Tag2" }
                },
                new Scenario
                {
                    Entity = new Product
                    {
                        Id = "Id",
                        Title = "Title",
                        SimilarProducts = new []
                        {
                            new Product
                            {
                                Id = "AlternativeId",
                                Title = "Alternative"
                            }
                        }

                    },
                    ExpectedResult = new[] { "Title", "Alternative" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public ICosmosEntity Entity { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }
        }

        public class Product : ICosmosEntity
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public int Stock { get; set; }
            public bool OutOfStock { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<Product> SimilarProducts { get; set; }
        }
    }
}
