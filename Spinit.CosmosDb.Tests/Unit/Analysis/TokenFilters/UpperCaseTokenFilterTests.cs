using Shouldly;
using Spinit.CosmosDb.Tests.Unit.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Analysis.TokenFilters
{
    public class UppercaseTokenFilterTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExecuteShouldReturnExpectedValue(Scenario scenario)
        {
            var filter = new UppercaseTokenFilter();
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
                    ExpectedResult = new[] { "ABC" }
                },
                new Scenario
                {
                    Input = new [] { "abc" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "ABC" }
                },
                new Scenario
                {
                    Input = new [] { "ABC" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "ABC" }
                },
                new Scenario
                {
                    Input = new [] { "ABC" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "ABC" }
                },
                new Scenario
                {
                    Input = new [] { "AbC", "dEf" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "ABC", "DEF" }
                },
                new Scenario
                {
                    Input = new [] { "AbC", "dEf" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "ABC", "DEF" }
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
