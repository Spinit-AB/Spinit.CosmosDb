using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Spinit.CosmosDb.Internals
{
    internal static class Bulk
    {
        internal class Operations<T> : List<Task<OperationResponse<T>>>
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

        internal class OperationsResponse<T> : ICosmosBulkOperationResult
        {
            public TimeSpan TotalTimeTaken { get; set; }
            public int SuccessfulDocuments { get; set; } = 0;
            public double TotalRequestUnitsConsumed { get; set; } = 0;

            public IReadOnlyList<(T, Exception)> Failures { get; set; }
        }

        internal class OperationResponse<T>
        {
            public T Item { get; set; }
            public double RequestUnitsConsumed { get; set; } = 0;
            public bool IsSuccessful { get; set; }
            public Exception CosmosException { get; set; }
        }

        internal static Task<OperationResponse<T>> CaptureOperationResponse<T>(Task<ItemResponse<T>> task, T item) => CaptureOperationResponse(task, item, origin => origin);

        internal static async Task<OperationResponse<T2>> CaptureOperationResponse<T1, T2>(Task<ItemResponse<T1>> task, T1 item, Func<T1, T2> itemTransformation)
        {
            try
            {
                var response = await task.ConfigureAwait(false);

                return new OperationResponse<T2>()
                {
                    Item = itemTransformation(item),
                    IsSuccessful = true,
                    RequestUnitsConsumed = task.Result.RequestCharge
                };
            }
            catch (CosmosException ex)
            {
                return new OperationResponse<T2>()
                {
                    Item = itemTransformation(item),
                    RequestUnitsConsumed = ex.RequestCharge,
                    IsSuccessful = false,
                    CosmosException = ex
                };
            }
            catch (Exception ex)
            {
                return new OperationResponse<T2>()
                {
                    Item = itemTransformation(item),
                    IsSuccessful = false,
                    CosmosException = ex
                };
            }
        }
    }


}
