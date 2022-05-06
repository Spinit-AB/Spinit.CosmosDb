using Microsoft.Azure.Cosmos;
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
        public void TestIsValidManualThoughput(int throughput, bool expected)
        {
            var isValid = ThroughputValidator.IsValidThroughput(ThroughputProperties.CreateManualThroughput(throughput), out _);
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [InlineData(100, false)]
        [InlineData(400, false)]
        [InlineData(1000, true)]
        [InlineData(4000, true)]
        [InlineData(10000, true)]
        [InlineData(1000000, true)]
        [InlineData(1000001, false)]
        public void TestIsValidAutoscaleThoughput(int throughput, bool expected)
        {
            var isValid = ThroughputValidator.IsValidThroughput(ThroughputProperties.CreateAutoscaleThroughput(throughput), out _);
            Assert.Equal(expected, isValid);
        }
    }
}
