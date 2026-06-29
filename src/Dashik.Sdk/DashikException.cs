namespace Dashik.Sdk;

/// <summary>
/// Application-related exception.
/// </summary>
public class DashikException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public DashikException()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    public DashikException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Message text.</param>
    /// <param name="innerException">Inner exception.</param>
    public DashikException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
