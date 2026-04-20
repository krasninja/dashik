using System.Collections.Concurrent;

namespace Dashik.Shared.Infrastructure;

/// <summary>
/// Dispatcher that runs parallel actions and limit degree of parallelism.
/// </summary>
internal sealed class ParallelDispatcher : IDisposable
{
    private readonly int _maxDegreeOfParallelism;
    private readonly ConcurrentQueue<ExecutionItem> _executionQueue = new();
    private readonly HashSet<Task> _runningTasks = new();
    private readonly Lock _runningTasksLock = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private volatile bool _isStarted;
    private long _runningCount;
    private bool _disposed;

    private sealed class ExecutionItem(Func<object?, CancellationToken, Task> action, object? state = null)
    {
        public Func<object?, CancellationToken, Task> Action { get; } = action;

        public object? State { get; } = state;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Max degree of parallelism.</param>
    /// <param name="start">Start immediately.</param>
    public ParallelDispatcher(int maxDegreeOfParallelism, bool start = false)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        if (start)
        {
            Start();
        }
    }

    /// <summary>
    /// Queue item.
    /// </summary>
    /// <param name="action">Action to queue.</param>
    /// <param name="state">State to pass to the action.</param>
    public void Queue(Func<object?, CancellationToken, Task> action, object? state = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ParallelDispatcher));
        _executionQueue.Enqueue(new ExecutionItem(action, state));
        ProcessQueue();
    }

    /// <summary>
    /// Queue item.
    /// </summary>
    /// <param name="action">Action to queue.</param>
    /// <param name="state">State to pass to the action.</param>
    public void Queue(Action<object?> action, object? state = null)
    {
        Queue((s, _) =>
        {
            action(s);
            return Task.CompletedTask;
        }, state);
    }

    private void ProcessQueue()
    {
        if (_disposed)
        {
            return;
        }

        while (_isStarted
            && Interlocked.Read(ref _runningCount) < _maxDegreeOfParallelism
            && _executionQueue.TryDequeue(out var item))
        {
            Interlocked.Increment(ref _runningCount);

            var cancellationToken = _cancellationTokenSource.Token;
            var currentItem = item; // Local copy to avoid captured variable warning.
            Task? task = null;
            task = Task.Run(async () =>
            {
#pragma warning disable AccessToModifiedClosure // Intentional: task is assigned before lambda executes
                var currentTask = task;
#pragma warning restore AccessToModifiedClosure
                try
                {
                    await currentItem.Action.Invoke(currentItem.State, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is expected, don't log.
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{nameof(ParallelDispatcher)}: Unhandled exception: {ex}");
                }
                finally
                {
                    Interlocked.Decrement(ref _runningCount);
                    lock (_runningTasksLock)
                    {
                        if (currentTask != null)
                        {
                            _runningTasks.Remove(currentTask);
                        }
                    }
                    ProcessQueue();
                }
            }, cancellationToken);

            lock (_runningTasksLock)
            {
                _runningTasks.Add(task);
            }
        }
    }

    /// <summary>
    /// Start queue processing.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ParallelDispatcher));
        _isStarted = true;
        ProcessQueue();
    }

    /// <summary>
    /// Pause/stop queue processing.
    /// </summary>
    public void Stop()
    {
        _isStarted = false;
    }

    /// <summary>
    /// Cancel all pending tasks.
    /// </summary>
    public async Task CancelAsync()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        await _cancellationTokenSource.CancelAsync();

        Task[] tasksToWait;
        lock (_runningTasksLock)
        {
            tasksToWait = _runningTasks.ToArray();
        }

        await Task.WhenAll(tasksToWait);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();

        Task[] tasksToWait;
        lock (_runningTasksLock)
        {
            tasksToWait = _runningTasks.ToArray();
        }
        Task.WaitAll(tasksToWait);
        _cancellationTokenSource.Dispose();
    }
}
