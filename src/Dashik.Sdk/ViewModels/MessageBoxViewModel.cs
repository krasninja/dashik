using System.ComponentModel.DataAnnotations;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Dashik.Sdk.Mvvm;

namespace Dashik.Sdk.ViewModels;

/// <summary>
/// View model for message box.
/// </summary>
public class MessageBoxViewModel : ReactiveObject, ICloseableViewModel, IDialogViewModel<DialogResult>
{
    private static readonly IImage _infoIcon;
    private static readonly IImage _questionIcon;
    private static readonly IImage _exclamationIcon;
    private static readonly IImage _errorIcon;

    /// <summary>
    /// Message box caption.
    /// </summary>
    public string Caption
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = "Information";

    /// <summary>
    /// Message to be displayed for user.
    /// </summary>
    [Required]
    public string Message { get; }

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    /// <inheritdoc />
    public DialogResult ResultValue { get; private set; } = DialogResult.OK;

    /// <inheritdoc />
    public DialogResult Result { get; private set; } = DialogResult.OK;

    /// <summary>
    /// Message box action selected.
    /// </summary>
    public ReactiveCommand<DialogResult, Unit> ActionSelectCommand { get; }

    /// <summary>
    /// Copy message box text command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CopyCommand { get; }

    /// <summary>
    /// Message box icon.
    /// </summary>
    public IImage? Icon
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = _infoIcon;

    /// <summary>
    /// Show "OK" button.
    /// </summary>
    public bool ShowOkButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = true;

    /// <summary>
    /// Show "Yes" button.
    /// </summary>
    public bool ShowYesButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "No" button.
    /// </summary>
    public bool ShowNoButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "Cancel" button.
    /// </summary>
    public bool ShowCancelButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "Continue" button.
    /// </summary>
    public bool ShowContinueButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "Abort" button.
    /// </summary>
    public bool ShowAbortButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "Retry" button.
    /// </summary>
    public bool ShowRetryButton
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Show "Ignore" button.
    /// </summary>
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

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message box text.</param>
    /// <param name="caption">Message box title.</param>
    public MessageBoxViewModel(string message, string? caption = null)
    {
        ActionSelectCommand = ReactiveCommand.Create<DialogResult>(SetValueAndClose);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyText);

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

    private async Task CopyText()
    {
        var text = Message;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        var clipboard = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.Clipboard;
        if (clipboard == null)
        {
            return;
        }

        try
        {
            await clipboard.SetTextAsync(text);
        }
        catch (Exception)
        {
            // Ignore because it might produce unexpected exceptions.
        }
    }

    private void SetValueAndClose(DialogResult value)
    {
        ResultValue = value;
        Result = value;
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show only "OK" button.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
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

    /// <summary>
    /// Show only "OK" and "Cancel" buttons.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
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

    /// <summary>
    /// Show only "OK" button and error icon.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
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

    /// <summary>
    /// Show only "Yes" and "No" buttons.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
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

    /// <summary>
    /// Show only "Yes", "No" and "Cancel" buttons.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
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

    /// <summary>
    /// Show only "Abort", "Retry" and "Ignore" buttons.
    /// </summary>
    /// <returns>Instance of <see cref="MessageBoxViewModel" />.</returns>
    public MessageBoxViewModel SetAbortRetryIgnoreMode()
    {
        ShowOkButton = false;
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
