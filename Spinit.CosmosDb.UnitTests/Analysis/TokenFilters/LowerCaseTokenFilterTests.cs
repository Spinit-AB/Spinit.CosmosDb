using System.Collections.Generic;
using Spinit.CosmosDb.UnitTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Analysis.TokenFilters
{
    public class LowercaseTokenFilterTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void ExecuteShouldReturnExpectedValue(Scenario scenario)
        {
            var filter = new LowercaseTokenFilter();
            var result = filter.Execute(scenario.Input, scenario.AnalyzeContext);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = new [] { "abc" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "abc" }
                },
                new Scenario
                {
                    Input = new [] { "abc" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "abc" }
                },
                new Scenario
                {
                    Input = new [] { "ABC" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "abc" }
                },
                new Scenario
                {
                    Input = new [] { "ABC" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "abc" }
                },
                new Scenario
                {
                    Input = new [] { "AbC", "dEf" },
                    AnalyzeContext = AnalyzeContext.Entity,
                    ExpectedResult = new[] { "abc", "def" }
                },
                new Scenario
                {
                    Input = new [] { "AbC", "dEf" },
                    AnalyzeContext = AnalyzeContext.Query,
                    ExpectedResult = new[] { "abc", "def" }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public IEnumerable<string> Input { get; set; }
            public AnalyzeContext AnalyzeContext { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }
        }
    }
}
