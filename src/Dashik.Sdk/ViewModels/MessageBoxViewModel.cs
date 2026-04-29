using System.Reactive;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;

namespace Dashik.Sdk.ViewModels;

public class MessageBoxViewModel : ReactiveObject, ICloseableViewModel, IDialogViewModel<DialogResult>
{
    private static readonly IImage _infoIcon;
    private static readonly IImage _questionIcon;
    private static readonly IImage _exclamationIcon;
    private static readonly IImage _errorIcon;

    public string Caption
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = "Information";

    public string Message { get; }

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    /// <inheritdoc />
    public DialogResult ResultValue { get; private set; } = DialogResult.OK;

    /// <inheritdoc />
    public DialogResult Result { get; private set; } = DialogResult.OK;

    public ReactiveCommand<DialogResult, Unit> ActionSelectCommand { get; }

    public IImage? Icon
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = _infoIcon;

    public bool ShowOkButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = true;

    public bool ShowYesButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowNoButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowCancelButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowContinueButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowAbortButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowRetryButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool ShowIgnoreButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    static MessageBoxViewModel()
    {
        var resourceInclude = new ResourceInclude(new Uri("avares://Dashik.Sdk/Resources/MessageBox.axaml"))
        {
            Source = new Uri("avares://Dashik.Sdk/Resources/MessageBox.axaml")
        };

        _infoIcon = GetIconResource(resourceInclude, "FontAwesomeSolidCircleInfo", Brushes.Blue);
        _questionIcon = GetIconResource(resourceInclude, "FontAwesomeSolidCircleQuestion", Brushes.Blue);
        _exclamationIcon = GetIconResource(resourceInclude, "FontAwesomeSolidTriangleExclamation", Brushes.Yellow);
        _errorIcon = GetIconResource(resourceInclude, "FontAwesomeRegularCircleXMark", Brushes.Red);
    }

    public MessageBoxViewModel(string message, string? caption = null)
    {
        ActionSelectCommand = ReactiveCommand.Create<DialogResult>(SetValueAndClose);
        Caption = caption ?? Caption;

        Message = message;
    }

    private static Bitmap GetIconResource(ResourceInclude resourceInclude, string icon, IBrush brush)
    {
        if (resourceInclude.TryGetResource(icon, null, out var resource) && resource is StreamGeometry geometry)
        {
            return ConvertToBitmap(geometry, brush);
        }

        throw new InvalidOperationException($"The icon '{icon}' is not found in resources.");
    }

    private static Bitmap ConvertToBitmap(StreamGeometry geometry, IBrush brush)
    {
        var pixelSize = new PixelSize((int)geometry.Bounds.Width, (int)geometry.Bounds.Height);
        var bitmap = new RenderTargetBitmap(pixelSize);
        using var context = bitmap.CreateDrawingContext();
        context.DrawGeometry(brush, null, geometry);
        return bitmap;
    }

    private void SetValueAndClose(DialogResult value)
    {
        ResultValue = value;
        Result = value;
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    public MessageBoxViewModel SetOkMode()
    {
        ShowOkButton = true;
        ShowCancelButton = false;
        ShowYesButton = false;
        ShowNoButton = false;
        ShowContinueButton = false;
        ShowAbortButton = false;
        ShowRetryButton = false;
        ShowIgnoreButton = false;
        Icon = _infoIcon;

        return this;
    }

    public MessageBoxViewModel SetOkCancelMode()
    {
        ShowOkButton = true;
        ShowCancelButton = true;
        ShowYesButton = false;
        ShowNoButton = false;
        ShowContinueButton = false;
        ShowAbortButton = false;
        ShowRetryButton = false;
        ShowIgnoreButton = false;
        Icon = _infoIcon;

        return this;
    }

    public MessageBoxViewModel SetErrorMode()
    {
        ShowOkButton = true;
        ShowCancelButton = false;
        ShowYesButton = false;
        ShowNoButton = false;
        ShowContinueButton = false;
        ShowAbortButton = false;
        ShowRetryButton = false;
        ShowIgnoreButton = false;
        Icon = _errorIcon;

        return this;
    }

    public MessageBoxViewModel SetYesNoMode()
    {
        ShowOkButton = false;
        ShowCancelButton = false;
        ShowYesButton = true;
        ShowNoButton = true;
        ShowContinueButton = false;
        ShowAbortButton = false;
        ShowRetryButton = false;
        ShowIgnoreButton = false;
        Icon = _questionIcon;

        return this;
    }

    public MessageBoxViewModel SetYesNoCancelMode()
    {
        ShowOkButton = false;
        ShowCancelButton = true;
        ShowYesButton = true;
        ShowNoButton = true;
        ShowContinueButton = false;
        ShowAbortButton = false;
        ShowRetryButton = false;
        ShowIgnoreButton = false;
        Icon = _questionIcon;

        return this;
    }

    public MessageBoxViewModel SetAbortRetryIgnoreMode()
    {
        ShowOkButton = true;
        ShowCancelButton = false;
        ShowYesButton = false;
        ShowNoButton = false;
        ShowContinueButton = false;
        ShowAbortButton = true;
        ShowRetryButton = true;
        ShowIgnoreButton = true;
        Icon = _exclamationIcon;

        return this;
    }
}
