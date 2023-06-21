using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Spinit.CosmosDb.Tests.Core.Order
{
    public class TestCaseByAttributeOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            // this looks like crap but has to be done due to strange implementation by Xunit that seems to use some sort of dynamic proxy... 
            var result = testCases.OrderBy(x => x.TestMethod.Method
                    .GetCustomAttributes(typeof(TestOrderAttribute))
                    .Cast<ReflectionAttributeInfo>()
                    .Select(a => a.Attribute)
                    .Cast<TestOrderAttribute>()
                    .Single()
                    .Order
                ).ToArray();
            return result;
        }
    }
}
