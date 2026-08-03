namespace Dashik.Sdk;

/// <summary>
/// The exception occurs when user need to configure the widget. In that case we render "configuration" button.
/// </summary>
public class WidgetNotConfiguredException : WidgetException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public WidgetNotConfiguredException()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    public WidgetNotConfiguredException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    /// <param name="innerException">Inner exception.</param>
    public WidgetNotConfiguredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
