using System;

namespace Spinit.CosmosDb
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseIdAttribute : Attribute
    {
        public DatabaseIdAttribute(string databaseId)
        {
            DatabaseId = databaseId;
        }

        public string DatabaseId { get; }
    }
}
