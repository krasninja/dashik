using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Logging;
using Dashik.Shared.Infrastructure.Logging;

namespace Dashik.Shared.Infrastructure.Converters;

internal sealed class LogLevelBackgroundConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogItem logItem)
        {
            return null;
        }

        return logItem.LogLevel switch
        {
            LogLevel.Debug => Brushes.LightGray,
            LogLevel.Trace => Brushes.LightGray,
            LogLevel.Warning => Brushes.LightYellow,
            LogLevel.Error => Brushes.PaleVioletRed,
            _ => Brushes.Transparent,
        };
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
