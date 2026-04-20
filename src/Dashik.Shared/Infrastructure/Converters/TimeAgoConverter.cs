using System.Globalization;
using Avalonia.Data.Converters;
using Dashik.Shared.Utils;

namespace Dashik.Shared.Infrastructure.Converters;

internal sealed class TimeAgoConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return "?";
        }

        var timeSpan = value switch
        {
            DateTime dt => DateTime.UtcNow - dt.ToUniversalTime(),
            DateTimeOffset dt => DateTimeOffset.UtcNow - dt.ToUniversalTime(),
            TimeSpan ts => ts,
            _ => throw new NotSupportedException($"Unsupported type: {value.GetType()}.")
        };
        return TimeSpanUtils.TimeAgo(timeSpan);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
