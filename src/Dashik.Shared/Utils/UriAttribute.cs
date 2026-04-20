using System.ComponentModel.DataAnnotations;

namespace Dashik.Shared.Utils;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class UriAttribute : DataTypeAttribute
{
    /// <inheritdoc />
    public UriAttribute() : base(DataType.Url)
    {
    }

    /// <inheritdoc />
    public UriAttribute(string customDataType) : base(customDataType)
    {
    }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        return Uri.IsWellFormedUriString(value as string, UriKind.Absolute);
    }
}
