﻿using Shouldly;
using Spinit.CosmosDb.Tests.Unit.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Analysis.Tokenizers
{
    public class PatternTokenizerTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void TokenizeShouldReturnExpectedValue(Scenario scenario)
        {
            var tokenizer = string.IsNullOrEmpty(scenario.Pattern)
                ? new PatternTokenizer()
                : new PatternTokenizer(scenario.Pattern);
            var result = tokenizer.Tokenize(scenario.Input);
            result.ShouldBe(scenario.ExpectedResult);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = "The foo_bar_size's default is 5.",
                    ExpectedResult = new[] { "The", "foo_bar_size", "s", "default", "is", "5" }
                },
                new Scenario
                {
                    Input = "The foo_bar_size's default is 5.",
                    Pattern = "\\s+", // whitespace only
                    ExpectedResult = new[] { "The", "foo_bar_size's", "default", "is", "5." }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public required string Input { get; set; }
            public string? Pattern { get; set; }
            public required IEnumerable<string> ExpectedResult { get; set; }
        }
    }
}
