using System.Reactive.Linq;

namespace Dashik.Shared.Utils;

/// <summary>
/// Extensions for <see cref="IObservable{T}" />.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    /// Subscribes to the observable sequence and executes the provided asynchronous action for each emitted item.
    /// The actions are executed sequentially, ensuring that each action completes before the next one starts.
    /// </summary>
    /// <param name="source">Source observable.</param>
    /// <param name="action">Action to execute.</param>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Disposable.</returns>
    public static IDisposable SubscribeAsync<TResult>(this IObservable<TResult> source, Func<TResult, Task> action)
    {
        return source.Select(param => Observable.FromAsync(() => action.Invoke(param)))
            .Concat() // Ensures sequential execution.
            .Subscribe();
    }

    /// <summary>
    /// Subscribes to the observable sequence and executes the provided asynchronous action for each emitted item.
    /// The actions are executed sequentially, ensuring that each action completes before the next one starts.
    /// </summary>
    /// <param name="source">Source observable.</param>
    /// <param name="action">Action to execute.</param>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Disposable.</returns>
    public static IDisposable SubscribeAsync<TResult>(this IObservable<TResult> source, Func<TResult, CancellationToken, Task> action)
    {
        return source.Select(param => Observable.FromAsync(ct => action.Invoke(param, ct)))
            .Concat() // Ensures sequential execution.
            .Subscribe();
    }
}
