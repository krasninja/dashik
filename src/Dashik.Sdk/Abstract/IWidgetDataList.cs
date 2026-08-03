namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget can expose its data as list of objects. It can be used for data filtering.
/// </summary>
public interface IWidgetDataList
{
    /// <summary>
    /// Data collection.
    /// </summary>
    IReadOnlyList<object> DataList { get; set; }
}
