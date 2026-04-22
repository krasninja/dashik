using Dashik.Sdk.Models;

namespace Dashik.Abstractions;

/// <summary>
/// Packages storage.
/// </summary>
public interface IPackagesStorage
{
    /// <summary>
    /// Storage URI.
    /// </summary>
    string Uri { get; }

    /// <summary>
    /// Storage name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Package information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of packages.</returns>
    Task<IReadOnlyList<WidgetPackage>> GetAsync(CancellationToken cancellationToken = default);
}
