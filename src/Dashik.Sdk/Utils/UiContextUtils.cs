using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace Dashik.Sdk.Utils;

/// <summary>
/// UI-thread related utilities.
/// </summary>
public static class UiContextUtils
{
    /// <summary>
    /// Continue execution within UI thread.
    /// </summary>
    /// <returns>Instance of <see cref="SwitchToUiAwaitable" />.</returns>
    public static SwitchToUiAwaitable SwitchToUi() => new(Dispatcher.UIThread);

    /// <summary>
    /// Invoke action within UI thread.
    /// </summary>
    /// <param name="callback">Callback.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke(Action callback)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            callback.Invoke();
        }
        else
        {
            Dispatcher.UIThread.Invoke(callback);
        }
    }

    /// <summary>
    /// UI thread awaiting.
    /// </summary>
    public readonly struct SwitchToUiAwaitable : INotifyCompletion
    {
        private readonly Dispatcher _dispatcher;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SwitchToUiAwaitable(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        /// <summary>
        /// Get instance of awaiter.
        /// </summary>
        /// <returns>Awaiter.</returns>
        public SwitchToUiAwaitable GetAwaiter() => this;

        /// <summary>
        /// Get result.
        /// </summary>
        public void GetResult()
        {
        }

        /// <summary>
        /// Is already on UI thread.
        /// </summary>
        public bool IsCompleted => _dispatcher.CheckAccess();

        /// <inheritdoc />
        public void OnCompleted(Action continuation)
        {
            _dispatcher.
            Invoke(continuation);
        }
    }
}
