namespace Dashik.Shared.Infrastructure.Logging;

public sealed class RingBufferLogsStorage
{
    /// <summary>
    /// List of logs.
    /// </summary>
    public RingBufferObservableList<LogItem> Logs { get; } = new(200);

    private static readonly Lock _objLock = new();

    public void AddLog(LogItem logItem)
    {
        lock (_objLock)
        {
            Logs.Add(logItem);
        }
    }
}
