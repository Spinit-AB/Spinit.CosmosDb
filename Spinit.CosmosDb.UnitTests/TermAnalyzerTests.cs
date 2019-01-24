using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Spinit.CosmosDb.UnitTests
{
    public class TermAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(GetExpectedTerms))]
        public void AnalyzeShouldContainExpectedTerm(string input, string expectedTerm)
        {
            var result = TermAnalyzer.Analyze(input);
            Assert.Contains(result, x => x == expectedTerm);
        }

        [Theory]
        [MemberData(nameof(GetExpectedTerms))]
        public void AnalyzeShouldReturnLowerCase(string input, string expectedTerm)
        {
            var result = TermAnalyzer.Analyze(input);
            Assert.Contains(result, x => x == expectedTerm.ToLower());
        }

        [Theory]
        [MemberData(nameof(GetScenarions))]
        public void AnalyzeShouldNotReturnEmptyTerms(Scenario scenario)
        {
            var result = TermAnalyzer.Analyze(scenario.Input);
            Assert.DoesNotContain(result, x => string.IsNullOrEmpty(x));
        }

        [Theory]
        [MemberData(nameof(GetScenarions))]
        public void AnalyzeShouldReturnExpectedResult(Scenario scenario)
        {
            var result = TermAnalyzer.Analyze(scenario.Input);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<string, string> GetExpectedTerms()
        {
            var scenarios = GetScenarions().Select(x => x.First() as Scenario);
            var values = scenarios.SelectMany(x => x.ExpectedResult.Select(t => Tuple.Create(x.Input, t)));
            var result = new TheoryData<string, string>();
            foreach ((string input, string term) in values)
            {
                result.Add(input, term);
            }
            return result;
        }

        public static TheoryData<Scenario> GetScenarions()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = null,
                    ExpectedResult = Array.Empty<string>()
                },
                new Scenario
                {
                    Input = "",
                    ExpectedResult = Array.Empty<string>()
                },
                new Scenario
                {
                    Input = "a",
                    ExpectedResult = new[] { "a" }
                },
                new Scenario
                {
                    Input = " a ",
                    ExpectedResult = new[] { "a" }
                },
                new Scenario
                {
                    Input = "a b c d e",
                    ExpectedResult = new[] { "a", "b", "c", "d", "e" }
                },
                new Scenario
                {
                    Input = "AB CDE F",
                    ExpectedResult = new[] { "ab", "cde", "f" }
                },
                new Scenario
                {
                    Input = " a  b   c  d e ",
                    ExpectedResult = new[] { "a", "b", "c", "d", "e" }
                }
            };
        }

        public class Scenario : IXunitSerializable
        {
            public string Input { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                JsonConvert.PopulateObject(info.GetValue<string>("data"), this);
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("data", JsonConvert.SerializeObject(this));
            }
        }
    }
}
