using System.Collections.ObjectModel;
using Dashik.Sdk.Models;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Show additional labels in the widget title.
/// </summary>
public interface IWidgetBadges
{
    /// <summary>
    /// Badges.
    /// </summary>
    ObservableCollection<WidgetBadge> Badges { get; }
}
