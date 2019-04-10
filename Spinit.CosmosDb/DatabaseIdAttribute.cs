using System;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Attribute used for declaring the name/id of the cosmos database
    /// </summary>
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
