using Spinit.CosmosDb.Validation;
using Xunit;

namespace Spinit.CosmosDb.UnitTests.Validation
{
    public class ThroughputValidationTests
    {
        [Theory]
        [InlineData(100, false)]
        [InlineData(300, false)]
        [InlineData(399, false)]
        [InlineData(400, true)]
        [InlineData(550, false)]
        [InlineData(10000, true)]
        [InlineData(1000000, true)]
        [InlineData(1000001, false)]
        public void TestIsValidThoughput(int throughput, bool expected)
        {
            var isValid = ThroughputValidator.IsValidThroughput(throughput);
            Assert.Equal(expected, isValid);
        }
    }
}
