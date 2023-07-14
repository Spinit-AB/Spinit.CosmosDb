using System;

namespace Spinit.CosmosDb
{
    public interface ICosmosBulkOperationResult
    {
        TimeSpan TotalTimeTaken { get; }
        int SuccessfulDocuments { get; }
        double TotalRequestUnitsConsumed { get; }
    }
}
