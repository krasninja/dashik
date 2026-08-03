using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Primitives;
using Dashik.Sdk.Mvvm;
using Dashik.Host.Infrastructure.Logging;
using Dashik.Host.Infrastructure.UI;

namespace Dashik.Host.ViewModels;

public sealed class LogsViewModel : ViewModelBase, ICloseableViewModel
{
    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    public RingBufferObservableList<LogItem> Logs { get; }

    public ObservableCollection<LogItem> FilteredLogs
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public LogItem? SelectLog
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string[] LogLevels { get; }

    public string LogLevelFilter
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            FilteredLogs = GetFilteredLogs();
        }
    }

    public string CategoryFilter
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            FilteredLogs = GetFilteredLogs();
        }
    }
    = string.Empty;

    public ReactiveCommand<RxVoid, RxVoid> CloseCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> ClearCommand { get; }

    public LogsViewModel(RingBufferLoggerProvider loggerProvider)
    {
        Logs = loggerProvider.Storage.Logs;
        FilteredLogs = GetFilteredLogs();

        LogLevels = new[] { "*" }.Concat(Enum.GetNames(typeof(LogLevel))).ToArray();
        LogLevelFilter = LogLevels[0];

        CloseCommand = ReactiveCommand.Create(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        ClearCommand = ReactiveCommand.Create(() =>
        {
            Logs.Clear();
            FilteredLogs.Clear();
        });
    }

    private ObservableCollection<LogItem> GetFilteredLogs()
    {
        var filtered = Logs.Where(log =>
            {
                var logLevelFilter = LogLevel.Information;
                if (Enum.TryParse(LogLevelFilter, out LogLevel parseLogLevelFilter))
                {
                    logLevelFilter = parseLogLevelFilter;
                }
                var levelFilter = LogLevelFilter == "*" || log.LogLevel == logLevelFilter;
                var categoryFilter = string.IsNullOrEmpty(CategoryFilter)
                                     || log.CategoryName.Contains(CategoryFilter, StringComparison.OrdinalIgnoreCase);
                return levelFilter && categoryFilter;
            })
            .OrderByDescending(l => l.Time);
        return new ObservableCollection<LogItem>(filtered);
    }
}
