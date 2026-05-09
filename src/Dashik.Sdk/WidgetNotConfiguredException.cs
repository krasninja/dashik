namespace Dashik.Sdk;

/// <summary>
/// The exception occurs when user need to configure the widget. In that case we render "configuration" button.
/// </summary>
public class WidgetNotConfiguredException : WidgetException
{
    public WidgetNotConfiguredException()
    {
    }

    public WidgetNotConfiguredException(string message) : base(message)
    {
    }

    public WidgetNotConfiguredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
