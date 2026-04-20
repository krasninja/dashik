using System.Globalization;
using Avalonia.Data.Converters;

namespace Dashik.Shared.Infrastructure.Converters;

internal sealed class MultiplyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue && parameter is string stringParam && double.TryParse(stringParam, out double multiplier))
        {
            return doubleValue * multiplier;
        }

        if (value is int intValue && parameter is string stringParam2 && double.TryParse(stringParam2, out double multiplier2))
        {
            return intValue * multiplier2;
        }

        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
