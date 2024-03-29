﻿using Shouldly;
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
            result.ShouldBe(scenario.ExpectedResult);
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
                        CreatedDate = new DateTime(2019, 4, 16, 12, 0, 0),
                        SeeAlso = Enumerable.Empty<BlogEntry>(),
                    },
                    ExpectedResult = new[] { "2019-04-16 12:00:00" },
                },
                new Scenario
                {
                    Entity = new BlogEntry
                    {
                        Id = "Id",
                        Title = "Title",
                        CreatedDate = new DateTime(2019, 4, 16, 12, 0, 0),
                        ModifiedDate = new DateTime(2019, 4, 16, 12, 30, 0),
                        SeeAlso = Enumerable.Empty<BlogEntry>(),
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
                                Title = "Some blog",
                                CreatedDate = new DateTime(2019, 4, 15, 12, 0, 0),
                                SeeAlso = Enumerable.Empty<BlogEntry>(),
                            }
                        }
                    },
                    ExpectedResult = new[] { "2019-04-16 12:00:00", "2019-04-15 12:00:00" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public required ICosmosEntity Entity { get; set; }
            public required IEnumerable<string> ExpectedResult { get; set; }
        }

        public class BlogEntry : ICosmosEntity
        {
            public required string Id { get; set; }
            public required string Title { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public required IEnumerable<BlogEntry> SeeAlso { get; set; }
        }
    }
}
