using System;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Attribute used for declaring the name/id of the cosmos db collection
    /// </summary>
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
