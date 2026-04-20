using System.Text.Json;
using System.Text.Json.Nodes;
using Dashik.Sdk.Abstract;

namespace Dashik.Sdk.Widgets;

/// <summary>
/// Widget initialization data.
/// </summary>
public sealed class WidgetInitInfo
{
    /// <summary>
    /// Widget instance.
    /// </summary>
    public IWidgetContext Context { get; }

    /// <summary>
    /// Widget settings.
    /// </summary>
    public JsonObject Settings { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">Widget context.</param>
    /// <param name="settings">Settings in JSON format.</param>
    public WidgetInitInfo(IWidgetContext context, JsonObject settings)
    {
        Context = context;
        Settings = settings;
    }

    /// <summary>
    /// Deserialize settings as type or create new.
    /// </summary>
    /// <typeparam name="T">Settings type.</typeparam>
    /// <returns>Instance of setting.</returns>
    public T GetSettings<T>() where T : class => (T)GetSettings(typeof(T));

    /// <summary>
    /// Deserialize settings as type or create new.
    /// </summary>
    /// <returns>Instance of setting.</returns>
    public object GetSettings(Type type)
    {
        try
        {
            return Settings.Deserialize(type) ?? Activator.CreateInstance(type)!;
        }
        catch (JsonException)
        {
            return Activator.CreateInstance(type)!;
        }
    }
}
