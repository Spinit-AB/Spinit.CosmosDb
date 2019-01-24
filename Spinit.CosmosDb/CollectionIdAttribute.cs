using System;

namespace Spinit.CosmosDb
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionIdAttribute : Attribute
    {
        public CollectionIdAttribute(string collectionId)
        {
            CollectionId = collectionId;
        }

        public string CollectionId { get; }
    }
}
