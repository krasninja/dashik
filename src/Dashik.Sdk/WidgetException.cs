namespace Dashik.Sdk;

/// <summary>
/// Widget exception. Used to indicate errors related to widgets, such as widget creation or update errors.
/// The exception can be shown to the user.
/// </summary>
public sealed class WidgetException : DashikException
{
    public WidgetException()
    {
    }

    public WidgetException(string message) : base(message)
    {
    }

    public WidgetException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
