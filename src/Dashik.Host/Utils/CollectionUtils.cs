using System.Collections.Specialized;
using Avalonia.Collections;

namespace Dashik.Host.Utils;

/// <summary>
/// Utilities for collections.
/// </summary>
internal static class CollectionUtils
{
    /// <summary>
    /// The method syncs changes from <see cref="NotifyCollectionChangedEventArgs" />
    /// to the destination collection. The generic type must be equal.
    /// </summary>
    /// <param name="ev">Instance of <see cref="NotifyCollectionChangedEventArgs" />.</param>
    /// <param name="dest">Destination list.</param>
    /// <typeparam name="T">Type of list.</typeparam>
    public static void SyncFromChangedEventArg<T>(NotifyCollectionChangedEventArgs ev, IList<T> dest)
    {
        switch (ev.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (ev.NewItems != null)
                {
                    var index = ev.NewStartingIndex;
                    foreach (T item in ev.NewItems)
                    {
                        if (index < 0 || index >= dest.Count)
                        {
                            dest.Add(item);
                        }
                        else
                        {
                            dest.Insert(index++, item);
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (ev.OldItems != null)
                {
                    var index = ev.OldStartingIndex;
                    if (index >= 0)
                    {
                        for (var i = ev.OldItems.Count - 1; i >= 0; i--)
                        {
                            dest.RemoveAt(index + i);
                        }
                    }
                    else
                    {
                        foreach (T item in ev.OldItems)
                        {
                            dest.Remove(item);
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (ev.NewItems != null && ev.OldStartingIndex >= 0)
                {
                    var index = ev.OldStartingIndex;
                    foreach (T item in ev.NewItems)
                    {
                        dest[index++] = item;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Move:
                if (ev.OldItems != null && ev.OldStartingIndex >= 0 && ev.NewStartingIndex >= 0)
                {
                    var movedItems = ev.OldItems.Cast<T>();
                    for (var i = ev.OldItems.Count - 1; i >= 0; i--)
                    {
                        dest.RemoveAt(ev.OldStartingIndex + i);
                    }

                    var insertIndex = ev.NewStartingIndex;
                    foreach (var item in movedItems)
                    {
                        dest.Insert(insertIndex++, item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                dest.Clear();
                break;
        }
    }

    /// <summary>
    /// Adds range of elements to collection.
    /// </summary>
    /// <param name="collection">Collection to update.</param>
    /// <param name="elements">Elements to add.</param>
    /// <typeparam name="T">Element type.</typeparam>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> elements)
    {
        if (collection is List<T> list)
        {
            list.AddRange(elements);
            return;
        }
        if (collection is AvaloniaList<T> avaloniaList)
        {
            avaloniaList.AddRange(elements);
            return;
        }

        foreach (T element in elements)
        {
            collection.Add(element);
        }
    }
}
