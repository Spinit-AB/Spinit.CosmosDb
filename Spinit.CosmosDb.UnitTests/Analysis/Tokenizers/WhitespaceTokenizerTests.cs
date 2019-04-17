using System.Collections.Generic;
using Spinit.CosmosDb.UnitTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Analysis.Tokenizers
{
    public class WhitespaceTokenizerTests
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void TokenizeShouldReturnExpectedValue(Scenario scenario)
        {
            var tokenizer = new WhitespaceTokenizer();
            var result = tokenizer.Tokenize(scenario.Input);
            Assert.Equal(scenario.ExpectedResult, result);
        }

        public static TheoryData<Scenario> GetScenarios()
        {
            return new TheoryData<Scenario>
            {
                new Scenario
                {
                    Input = "The foo_bar_size's default is 5.",
                    ExpectedResult = new[] { "The", "foo_bar_size's", "default", "is", "5." }
                }
            };
        }

        public class Scenario : XunitSerializable
        {
            public string Input { get; set; }
            public IEnumerable<string> ExpectedResult { get; set; }
        }
    }
}
