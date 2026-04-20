using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Dashik.Sdk.Utils;

/// <summary>
/// Description attribute that provides value from resource file.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ResourceDescriptionAttribute : DescriptionAttribute
{
    private readonly ResourceManager _resourceManager;

    public ResourceDescriptionAttribute(Type resourceType, string resourceKey)
    {
        _resourceManager = new ResourceManager(resourceType);
        Description = resourceKey;
    }

    /// <inheritdoc />
    public override string Description =>
        _resourceManager.GetString(field, CultureInfo.CurrentUICulture) ?? field;
}
