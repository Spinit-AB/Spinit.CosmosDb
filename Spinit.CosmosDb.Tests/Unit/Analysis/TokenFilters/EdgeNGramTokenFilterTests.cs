using Shouldly;
using Spinit.CosmosDb.Tests.Unit.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Analysis.TokenFilters
{
    public class EdgeNGramTokenFilterTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExecuteShouldReturnExpectedValue(Scenario scenario)
        {
            var filter = new EdgeNGramTokenFilter();
            var result = filter.Execute(scenario.Input, scenario.AnalyzeContext);
            result.ShouldBe(scenario.ExpectedResult);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = new [] { "abc" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "a", "ab", "abc" }
                },
                new Scenario
                {
                    Input = new [] { "abc" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "abc" }
                },
                new Scenario
                {
                    Input = new [] { "abc", "def" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "a", "ab", "abc", "d", "de", "def" }
                },
                new Scenario
                {
                    Input = new [] { "abc", "def" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "abc", "def" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public required IEnumerable<string> Input { get; set; }
            public required AnalyzeContext AnalyzeContext { get; set; }
            public required IEnumerable<string> ExpectedResult { get; set; }
        }
    }
}
