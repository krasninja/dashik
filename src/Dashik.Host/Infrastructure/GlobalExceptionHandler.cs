using Avalonia;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dashik.Host.Infrastructure;

/// <summary>
/// Global RX exception handler.
/// </summary>
public sealed class GlobalExceptionHandler : IObserver<Exception>
{
    /// <inheritdoc />
    public void OnCompleted()
    {
    }

    /// <inheritdoc />
    public async void OnError(Exception error)
    {
        await ProcessException(error);
    }

    /// <inheritdoc />
    public async void OnNext(Exception value)
    {
        await ProcessException(value);
    }

    private async Task ProcessException(Exception exception)
    {
        if (Application.Current == null)
        {
            return;
        }

        if (Application.Current is not App app)
        {
            return;
        }

        var mvvmService = app.Container.GetRequiredService<IMvvmService>();
        var logger = app.Container.GetRequiredService<ILogger<GlobalExceptionHandler>>();
        logger.LogError(exception, "Unhandled exception occurred.");
        var messageBox = new MessageBoxViewModel(exception.Message, "Error").SetErrorMode();
        await mvvmService.OpenAsync(messageBox);
    }
}
