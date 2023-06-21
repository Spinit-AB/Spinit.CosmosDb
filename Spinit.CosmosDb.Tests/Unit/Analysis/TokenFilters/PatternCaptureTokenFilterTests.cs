using System.Collections.Generic;
using Spinit.CosmosDb.Tests.Unit.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Analysis.TokenFilters
{
    public class PatternCaptureTokenFilterTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExecuteShouldReturnExpectedValue(Scenario scenario)
        {
            var filter = new PatternCaptureTokenFilter(scenario.Pattern, scenario.PreserveOriginal);
            var result = filter.Execute(scenario.Input, scenario.AnalyzeContext);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = new [] { "ABC123" },
                    Pattern = "(\\d+)|(\\D+)",
                    PreserveOriginal = true,
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "ABC", "123", "ABC123" }
                },
                new Scenario
                {
                    Input = new [] { "ABC123" },
                    Pattern = "(\\d+)|(\\D+)",
                    PreserveOriginal = false,
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "ABC", "123" }
                },
                new Scenario
                {
                    Input = new [] { "martin.oom@spinit.se" },
                    Pattern = "(?<user>[^@]+)@(?<domain>[^@]+)",
                    PreserveOriginal = false,
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "martin.oom", "spinit.se" }
                },
                new Scenario
                {
                    Input = new [] { "martin.oom@spinit.se" },
                    Pattern = "([^@]+)",
                    PreserveOriginal = false,
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "martin.oom", "spinit.se" }
                },
                new Scenario
                {
                    Input = new [] { "abc123def456" },
                    Pattern = "(([a-z]+)(\\d*))",
                    PreserveOriginal = false,
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "abc123", "abc", "123", "def456", "def", "456" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public IEnumerable<string> Input { get; set; }
            public string Pattern { get; set; }
            public bool PreserveOriginal { get; set; }
            public AnalyzeContext AnalyzeContext { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }
        }
    }
}
