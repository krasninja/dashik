using System.Text.Json.Nodes;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;

namespace Dashik.Abstractions;

public interface IWidgetInstance : IWidgetContext
{
    /// <summary>
    /// Widget unique identifier. It is used to distinguish widget instances and to save their state.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Widget related settings. Unique for each widget instance.
    /// </summary>
    JsonObject WidgetSettings { get; set; }

    /// <summary>
    /// Main widget settings. It is used to store common settings for all widgets (such as title, visibility, etc.).
    /// </summary>
    WidgetMainSettings MainSettings { get; set; }

    /// <summary>
    /// Widget information. It is used to create widget instance and to display widget name in UI.
    /// </summary>
    WidgetInfo Info { get; }
}
