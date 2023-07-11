using System.Runtime.CompilerServices;

namespace Spinit.CosmosDb.Tests.Core.Order
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TestOrderAttribute : Attribute
    {
        public TestOrderAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }

        public int Order { get; private set; }
    }
}
