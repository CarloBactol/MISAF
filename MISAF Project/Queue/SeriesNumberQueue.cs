using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;
using System.Web;
using MISAF_Project.Services;

namespace MISAF_Project.Queue
{
    /// <summary>
    /// A static class that manages a queue for generating sequential series numbers in a thread-safe manner.
    /// This class ensures that series numbers are generated sequentially even when multiple users or threads
    /// request numbers simultaneously, preventing race conditions and duplicate series numbers.
    /// It uses a <see cref="ConcurrentQueue{T}"/> to queue requests and processes them one at a time.
    /// </summary>
    public static class SeriesNumberQueue
    {
        /// <summary>
        /// A thread-safe queue that holds requests for series numbers. Each item in the queue is a tuple
        /// containing the table name and a <see cref="TaskCompletionSource{T}"/> to signal the result.
        /// </summary>
        private static readonly ConcurrentQueue<(string TableName, TaskCompletionSource<int> Tcs)> _queue = new ConcurrentQueue<(string, TaskCompletionSource<int>)>();

        /// <summary>
        /// A cancellation token source used to signal when the queue processing should stop.
        /// </summary>
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// A flag indicating whether the queue is currently being processed. This prevents multiple
        /// processing tasks from running concurrently.
        /// </summary>
        private static bool _isProcessing = false;

        /// <summary>
        /// The service responsible for retrieving and updating series numbers from the database.
        /// </summary>
        private static ILastSeriesService _lastSeriesService;

        /// <summary>
        /// Initializes the <see cref="SeriesNumberQueue"/> with the specified <see cref="ILastSeriesService"/>.
        /// This method starts a background task to process the queue of series number requests.
        /// </summary>
        /// <param name="lastSeriesService">The service used to retrieve and update series numbers.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="lastSeriesService"/> is null.</exception>
        public static void Initialize(ILastSeriesService lastSeriesService)
        {
            if (lastSeriesService == null)
                throw new ArgumentNullException(nameof(lastSeriesService));

            _lastSeriesService = lastSeriesService;
            Task.Run(() => ProcessQueue(_cts.Token));
        }

        /// <summary>
        /// Requests the next series number for the specified table. This method adds the request to the queue
        /// and returns a <see cref="Task{T}"/> that will complete when the series number is available.
        /// </summary>
        /// <param name="tableName">The name of the table for which to generate the next series number (e.g., "MAF Main").</param>
        /// <returns>A <see cref="Task{T}"/> that represents the asynchronous operation and returns the next series number.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tableName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the queue has not been initialized with a valid <see cref="ILastSeriesService"/>.</exception>
        public static Task<int> GetNextSeriesAsync(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (_lastSeriesService == null)
                throw new InvalidOperationException("SeriesNumberQueue has not been initialized. Call Initialize() first.");

            var tcs = new TaskCompletionSource<int>();
            _queue.Enqueue((tableName, tcs));
            return tcs.Task;
        }

        /// <summary>
        /// Processes the queue of series number requests in a loop until cancellation is requested.
        /// This method dequeues requests one at a time, retrieves the next series number using the
        /// <see cref="ILastSeriesService"/>, and sets the result on the associated <see cref="TaskCompletionSource{T}"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to stop queue processing.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task ProcessQueue(CancellationToken cancellationToken)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var request))
                {
                    var (tableName, tcs) = request;

                    try
                    {
                        int newSeries = await _lastSeriesService.GetNextSeriesAsync(tableName);
                        tcs.SetResult(newSeries);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }

            _isProcessing = false;
        }

        /// <summary>
        /// Stops the queue processing by signaling cancellation. This method should be called
        /// during application shutdown to ensure the background task is gracefully terminated.
        /// </summary>
        public static void Stop()
        {
            _cts.Cancel();
        }
    }
}