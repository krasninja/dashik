using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Dashik.Shared.Infrastructure.Logging;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Sdk.Mvvm;

namespace Dashik.Shared.ViewModels;

public sealed class LogsViewModel : ViewModelBase, ICloseableViewModel
{
    private readonly RingBufferLoggerProvider _loggerProvider;

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

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

    public LogsViewModel(RingBufferLoggerProvider loggerProvider)
    {
        _loggerProvider = loggerProvider;
        Logs = _loggerProvider.Storage.Logs;
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
