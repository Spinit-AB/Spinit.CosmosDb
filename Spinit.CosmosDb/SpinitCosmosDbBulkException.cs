using System;
using System.Collections.Generic;
using System.Linq;
using Spinit.CosmosDb.Internals;

namespace Spinit.CosmosDb
{
    public class SpinitCosmosDbBulkException : Exception, ICosmosBulkOperationResult
    {
        internal SpinitCosmosDbBulkException(string operation, IReadOnlyList<(object, Exception)> failures, int successfulDocuments, TimeSpan totalTimeTaken, double totalRequestUnitsConsumed) : base($"Bulk {operation} failed")
        {
            Failures = failures;
            SuccessfulDocuments = successfulDocuments;
            TotalTimeTaken = totalTimeTaken;
            TotalRequestUnitsConsumed = totalRequestUnitsConsumed;
        }

        public IReadOnlyList<(object Entry, Exception Exception)> Failures { get; }
        public int SuccessfulDocuments { get; }
        public TimeSpan TotalTimeTaken { get; }
        public double TotalRequestUnitsConsumed { get; }

        internal static SpinitCosmosDbBulkException Create<T>(string operation, Bulk.OperationsResponse<T> bulkOperationsResponse)
        {
            return new SpinitCosmosDbBulkException(
                operation,
                bulkOperationsResponse.Failures.Select(x => ((object)x.Item1, x.Item2)).ToList(),
                bulkOperationsResponse.SuccessfulDocuments,
                bulkOperationsResponse.TotalTimeTaken,
                bulkOperationsResponse.TotalRequestUnitsConsumed);
        }
    }
}
