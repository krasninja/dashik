using ReactiveUI;
using ReactiveUI.Primitives;
using Dashik.Sdk.Mvvm;

namespace Dashik.Sdk.ViewModels;

/// <summary>
/// View model for text window.
/// </summary>
public class TextWindowViewModel : ReactiveObject, ICloseableViewModel
{
    /// <summary>
    /// Window text.
    /// </summary>
    public string Text
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    public ReactiveCommand<RxVoid, RxVoid> CloseCommand { get; }

    public TextWindowViewModel()
    {
        CloseCommand = ReactiveCommand.Create(() => CloseRequest?.Invoke(this, EventArgs.Empty));
    }
}
