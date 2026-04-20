namespace Dashik.Sdk;

/// <summary>
/// Application-related exception.
/// </summary>
public class DashikException : Exception
{
    public DashikException()
    {
    }

    public DashikException(string message) : base(message)
    {
    }

    public DashikException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
