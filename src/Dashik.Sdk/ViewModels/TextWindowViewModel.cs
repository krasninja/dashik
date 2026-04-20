using System.Reactive;
using ReactiveUI;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;

namespace Dashik.Sdk.ViewModels;

public class TextWindowViewModel : ObservableObject, ICloseableViewModel
{
    public string Text
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public TextWindowViewModel()
    {
        CloseCommand = ReactiveCommand.Create(() => CloseRequest?.Invoke(this, EventArgs.Empty));
    }
}
