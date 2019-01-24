using System;

namespace Spinit.CosmosDb
{
    public interface ICosmosEntity
    {
        Guid Id { get; set; }
    }
}
