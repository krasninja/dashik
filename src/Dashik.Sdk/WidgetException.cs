namespace Dashik.Sdk;

/// <summary>
/// Widget exception. Used to indicate errors related to widgets, such as widget creation or update errors.
/// The exception can be shown to the user.
/// </summary>
public class WidgetException : DashikException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public WidgetException()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    public WidgetException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    /// <param name="innerException">Inner exception.</param>
    public WidgetException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
