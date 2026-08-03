using System.Collections;
using System.Reflection;

namespace Dashik.Host.Utils;

/// <summary>
/// Traverse internal object properties, enumerations, lists, dictionaries.
/// </summary>
internal sealed class SimpleObjectTraverser
{
    /// <summary>
    /// Ignore certain types from processing.
    /// </summary>
    public Type[] IgnoreTypes { get; set; } = [];

    /// <summary>
    /// Object traverse information.
    /// </summary>
    public readonly struct ObjectTraverseInfo(object? obj, object? source, PropertyInfo propertyInfo, object? tag)
    {
        /// <summary>
        /// The current object.
        /// </summary>
        public object? Object { get; } = obj;

        /// <summary>
        /// Source object (for property), list (for enumerable) or dictionary (for dictionary value).
        /// </summary>
        public object? Source { get; } = source;

        /// <summary>
        /// Property info if it is a property of the object.
        /// </summary>
        public PropertyInfo PropertyInfo { get; } = propertyInfo;

        /// <summary>
        /// Custom user tag.
        /// </summary>
        public object? Tag { get; } = tag;
    }

    /// <summary>
    /// Delegate for object traverse action. Return true to continue traversing, false to stop traversing.
    /// </summary>
    public delegate bool ObjectTraverseAction(in ObjectTraverseInfo info);

    public void Traverse(object? obj, ObjectTraverseAction action, object? tag = null)
    {
        if (obj == null)
        {
            return;
        }

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance) { obj };
        TraverseProperties(obj, action, visited, tag);
    }

    private bool TraverseProperties(object obj, ObjectTraverseAction action, HashSet<object> visited, object? tag)
    {
        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(obj);
            }
            catch
            {
                continue;
            }

            if (value == null || IsIgnored(value.GetType()))
            {
                continue;
            }

            if (value is IDictionary dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    if (!VisitChild(dictionary[key], dictionary, property, action, visited, tag))
                    {
                        return false;
                    }
                }
            }
            else if (value is IEnumerable enumerable && value is not string)
            {
                foreach (var item in enumerable)
                {
                    if (!VisitChild(item, enumerable, property, action, visited, tag))
                    {
                        return false;
                    }
                }
            }
            else if (!VisitChild(value, obj, property, action, visited, tag))
            {
                return false;
            }
        }

        return true;
    }

    private bool VisitChild(
        object? value,
        object? source,
        PropertyInfo propertyInfo,
        ObjectTraverseAction action,
        HashSet<object> visited,
        object? tag)
    {
        if (value == null || IsIgnored(value.GetType()))
        {
            return true;
        }

        if (!action(new ObjectTraverseInfo(value, source, propertyInfo, tag)))
        {
            return false;
        }

        if (IsSimpleType(value.GetType()) || !visited.Add(value))
        {
            return true;
        }

        return TraverseProperties(value, action, visited, tag);
    }

    private bool IsIgnored(Type type) => Array.IndexOf(IgnoreTypes, type) >= 0;

    public static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive
            || underlyingType.IsEnum
            || underlyingType == typeof(string)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(TimeSpan)
            || underlyingType == typeof(Guid);
    }
}
