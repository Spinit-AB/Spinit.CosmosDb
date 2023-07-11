using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb.Internals
{
    internal static class Bulk
    {
        public class Operations<T> : List<Task<OperationResponse<T>>>
        {
            private readonly Stopwatch stopwatch = Stopwatch.StartNew();

            public Operations(int operationCount) : base(operationCount) { }

            public async Task<OperationsResponse<T>> ExecuteAsync()
            {
                await Task.WhenAll(this).ConfigureAwait(false);
                stopwatch.Stop();
                return new OperationsResponse<T>()
                {
                    TotalTimeTaken = stopwatch.Elapsed,
                    TotalRequestUnitsConsumed = this.Sum(task => task.Result.RequestUnitsConsumed),
                    SuccessfulDocuments = this.Count(task => task.Result.IsSuccessful),
                    Failures = this.Where(task => !task.Result.IsSuccessful).Select(task => (task.Result.Item, task.Result.CosmosException)).ToList()
                };
            }
        }

        public class OperationsResponse<T>
        {
            public TimeSpan TotalTimeTaken { get; set; }
            public int SuccessfulDocuments { get; set; } = 0;
            public double TotalRequestUnitsConsumed { get; set; } = 0;

            public IReadOnlyList<(T, Exception)> Failures { get; set; }
        }

        public class OperationResponse<T>
        {
            public T Item { get; set; }
            public double RequestUnitsConsumed { get; set; } = 0;
            public bool IsSuccessful { get; set; }
            public Exception CosmosException { get; set; }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Used in bulk execution where execution is meant to continue even in the event of exceptions")]
        public static async Task<OperationResponse<T>> CaptureOperationResponse<T>(Task<ItemResponse<T>> task, T item)
        {
            try
            {
                ItemResponse<T> response = await task.ConfigureAwait(false);
                return new OperationResponse<T>()
                {
                    Item = item,
                    IsSuccessful = true,
                    RequestUnitsConsumed = task.Result.RequestCharge
                };
            }
            catch (CosmosException ex)
            {
                return new OperationResponse<T>()
                {
                    Item = item,
                    RequestUnitsConsumed = ex.RequestCharge,
                    IsSuccessful = false,
                    CosmosException = ex
                };
            }
            catch (Exception ex)
            {
                return new OperationResponse<T>()
                {
                    Item = item,
                    IsSuccessful = false,
                    CosmosException = ex
                };
            }
        }
    }


}
