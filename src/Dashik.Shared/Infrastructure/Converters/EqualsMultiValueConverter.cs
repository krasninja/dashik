using System.Globalization;
using Avalonia.Data.Converters;

namespace Dashik.Shared.Infrastructure.Converters;

internal sealed class EqualsMultiValueConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        for (var i = 1; i < values.Count; i++)
        {
            if (!Equals(values[i - 1], values[i]))
            {
                return false;
            }
        }

        return true;
    }
}
