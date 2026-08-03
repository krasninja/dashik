using QueryCat.Backend.Core.Types;
using Dashik.Sdk.Models;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Show additional metrics that can be provided by widget. The short value can be rendered in the widget title.
/// </summary>
public interface IWidgetMetrics
{
    /// <summary>
    /// Values of metrics. Key is the id of the metric provided by <see cref="GetAvailableMetrics" /> method.
    /// The id can be customized by optional argument with "--" suffix (metric_id--argument),
    /// for example "ping--www.google.com". The value is the result of the metric query.
    /// The dictionary of metric values will be updated with <see cref="IWidgetUpdate" /> interface.
    /// </summary>
    IDictionary<string, VariantValue> MetricValues { get; }

    /// <summary>
    /// Get available metrics provided by widget.
    /// </summary>
    /// <returns>Collection of metrics.</returns>
    IReadOnlyCollection<WidgetMetric> GetAvailableMetrics();
}
