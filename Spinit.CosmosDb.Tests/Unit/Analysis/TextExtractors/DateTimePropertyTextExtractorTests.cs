using System;
using System.Collections.Generic;
using Spinit.CosmosDb.Tests.Unit.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Analysis.TextExtractors
{
    public class DateTimePropertyTextExtractorTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExtractTextShouldReturnExpectedValue(Scenario scenario)
        {
            var extractor = new DateTimePropertyTextExtractor("yyyy-MM-dd HH:mm:ss");
            var result = extractor.ExtractText(scenario.Entity);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Entity = new BlogEntry
                    {
                        Id = "Id",
                        Title = "Title",
                        CreatedDate = new DateTime(2019, 4, 16, 12, 0, 0)
                    },
                    ExpectedResult = new[] { "2019-04-16 12:00:00" }
                },
                new Scenario
                {
                    Entity = new BlogEntry
                    {
                        Id = "Id",
                        Title = "Title",
                        CreatedDate = new DateTime(2019, 4, 16, 12, 0, 0),
                        ModifiedDate = new DateTime(2019, 4, 16, 12, 30, 0)
                    },
                    ExpectedResult = new[] { "2019-04-16 12:00:00", "2019-04-16 12:30:00" }
                },
                new Scenario
                {
                    Entity = new BlogEntry
                    {
                        Id = "Id",
                        Title = "Title",
                        CreatedDate = new DateTime(2019, 4, 16, 12, 0, 0),
                        SeeAlso = new []
                        {
                            new BlogEntry
                            {
                                Id = "SeeAlsoId",
                                CreatedDate = new DateTime(2019, 4, 15, 12, 0, 0),
                            }
                        }
                    },
                    ExpectedResult = new[] { "2019-04-16 12:00:00", "2019-04-15 12:00:00" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public ICosmosEntity Entity { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }
        }

        public class BlogEntry : ICosmosEntity
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public IEnumerable<BlogEntry> SeeAlso { get; set; }
        }
    }
}
