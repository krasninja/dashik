using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashik.Shared.Utils;

/// <summary>
/// Simple reflection-based objects cloner as single class file with no dependencies.
/// </summary>
/// <remarks>
/// - It uses field-to-field copy instead of properties.
/// - (UseCopyConstructor) If clone constructor (the constructor with one parameter of the same type) is presented - it can be used.
/// - (UseIClonable) If type implements <see cref="ICloneable" /> interface - is can be used.
/// - (CopyEventHandlers) You can ignore event handlers copy.
/// - (IgnoreTypes) You can skip specific types copy. Also, IgnoreFunc delegate can be used.
/// - (AddSourceObjectMapping) During cloning you can substitute specific objects instead of clone.
/// - (AddObjectFactory) You can override object creation.
/// - (AfterClone) You can customize object after its creation.
/// - (AddPostCloneHandler) Register post full clone handler.
/// - (CloneIt.IgnoreObject) You can ignore field processing by returning ignore object.
/// - (MaxDepth) You can limit recursion depth.
/// - Read only fields copy is supported.
/// - Multi dimensional array coping is not supported.
/// - Do not use the same instance of cloner to clone multiple objects.
/// - The class is not thread safe.
/// </remarks>
public class CloneIt : IDisposable
{
    private static readonly MethodInfo _cloneMethod = typeof(object).GetMethod("MemberwiseClone",
        BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// The predefined instance of object to be used to ignore the field.
    /// </summary>
    public static object IgnoreObject { get; } = new();

    private readonly Dictionary<object, object?> _sourceDestinationMap = new(capacity: 64, ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Type, Func<InputInfo, object>> _objectFactories = new();
    private readonly Stack<InputInfo> _path = new();
    private readonly List<object> _selfRecursionPath = new();
    private readonly Dictionary<object, Action<object>> _postCloneHandlers = new(ReferenceEqualityComparer.Instance);

    private readonly List<object> _clonableIgnoreObjects = new();

    // Cache.

    private static readonly ConcurrentDictionary<Type, Func<object, CloneIt, object>?> _extendedCloneMethodTypes = new();
    private static readonly ConcurrentDictionary<Type, Func<IEnumerable>?> _collectionsConstructorTypes = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo?> _collectionsAddMethods = new();
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> _typeClonableFields = new();
    private static readonly ConcurrentDictionary<FieldInfo, PropertyInfo?> _fieldToProperties = new();

    /// <summary>
    /// Ignore cloning based on target <see cref="FieldInfo" />.
    /// </summary>
    public Func<FieldInfo, bool>? IgnoreFieldFunc { get; set; }

    /// <summary>
    /// Ignore cloning based on target object.
    /// </summary>
    public Func<object?, bool>? IgnoreObjectFunc { get; set; }

    /// <summary>
    /// The callback is called after object creation.
    /// </summary>
    public Func<InputInfo, object?, object?>? AfterClone { get; set; }

    /// <summary>
    /// Types to ignore.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<Type> IgnoreTypes { get; } = new();

    /// <summary>
    /// Use <see cref="ICloneable" /> interface if object supports it. <c>True</c> by default.
    /// </summary>
    /// <remarks>It's a global setting for all objects.</remarks>
    // ReSharper disable once IdentifierTypo
    public bool UseIClonable { get; set; } = true;

    /// <summary>
    /// Copy event handlers of events. <c>True</c> by default.
    /// </summary>
    public bool CopyEventHandlers { get; set; } = true;

    /// <summary>
    /// Should validate source object mapping type when setting mapping. <c>True</c> by default.
    /// </summary>
    public bool ShouldValidateSourceObjectMappingType { get; set; } = true;

    /// <summary>
    /// Use copy constructor to clone.
    /// </summary>
    public bool UseCopyConstructor { get; set; }

    /// <summary>
    /// Try to use properties to set values instead of fields. Can decrease performance.
    /// </summary>
    public bool UsePropertyCopy { get; set; }

    /// <summary>
    /// Max recursion depth.
    /// </summary>
    public int MaxDepth { get; set; } = 64;

    /// <summary>
    /// Custom cloner.
    /// </summary>
    public ClonerDelegate? CustomCloner { get; set; } = null;

    /// <summary>
    /// Input information for clone method.
    /// </summary>
    /// <param name="Source">Source object.</param>
    /// <param name="Type">Source type.</param>
    /// <param name="Depth">Recursion depth.</param>
    /// <param name="FieldInfo">Field info.</param>
    public readonly record struct InputInfo(
        object? Source,
        Type Type,
        int Depth = 0,
        FieldInfo? FieldInfo = null);

    /// <summary>
    /// Result information about clone.
    /// </summary>
    /// <param name="Handled">Was the source object was suitable for clone.</param>
    /// <param name="Destination">Cloned object or null.</param>
    public readonly record struct ResultInfo(bool Handled, object? Destination);

    /// <summary>
    /// Cloner delegate.
    /// </summary>
    public delegate ResultInfo ClonerDelegate(ref InputInfo inputInfo);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ResultInfo GetNoResultInfo() => new(false, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ResultInfo GetOkResultInfo(object? obj = null) => new(true, obj);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ResultInfo GetIgnoreResultInfo() => new(true, IgnoreObject);

    private readonly ClonerDelegate[] _cloneMethods;

    /// <summary>
    /// Constructor.
    /// </summary>
    public CloneIt()
    {
        _cloneMethods =
        [
            TryGetMapped,
            TryGetIsIgnored,
            TryCopyWithCustomCloner,
            TryCopyObjectFactory,
            TryCopySimpleObject,
            TryCopyDictionary,
            TryCopyEnumerable,
            TryCopyArray,
            TryCopySpecialType,
            TryGetCloneable,
            TryCopyWithCopyConstructor,
            TryCopyObject,
        ];
    }

    /// <summary>
    /// Is clonable ignore object.
    /// </summary>
    public bool IsClonableIgnoreObject(object obj)
        => _clonableIgnoreObjects.Contains(obj);

    /// <summary>
    /// Add clonable ignore object.
    /// </summary>
    /// <param name="ignoredObject">Ignored object.</param>
    public void AddClonableIgnoreObject(object ignoredObject)
        => _clonableIgnoreObjects.Add(ignoredObject);

    /// <summary>
    /// Remove clonable ignore object.
    /// </summary>
    /// <param name="ignoredObject">Ignored object.</param>
    public void RemoveClonableIgnoreObject(object ignoredObject)
        => _clonableIgnoreObjects.Remove(ignoredObject);

    #region Mapping

    /// <summary>
    /// Add custom source object to be replaced.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="destination">Destination object.</param>
    public void AddSourceObjectMapping(object source, object? destination)
    {
        if (_sourceDestinationMap.ContainsKey(source))
        {
            throw new ArgumentException($"Key {source} already exists.", nameof(source));
        }
        SetSourceObjectMapping(source, destination);
    }

    /// <summary>
    /// Get snapshot of source to destination object mapping.
    /// </summary>
    public IDictionary<object, object?> GetSourceObjectMappingSnapshot()
        => _sourceDestinationMap.ToFrozenDictionary();

    /// <summary>
    /// Setup source to destination object mapping from snapshot.
    /// </summary>
    public void SetupSourceObjectMappingFromSnapshot(IDictionary<object, object?> snapshot)
    {
        _sourceDestinationMap.Clear();

        foreach (var kvp in snapshot)
        {
            SetSourceObjectMapping(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Set custom source object to be replaced.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="destination">Destination object.</param>
    public void SetSourceObjectMapping(object source, object? destination)
    {
        if (ShouldValidateSourceObjectMappingType
            && destination is not null
            && destination != IgnoreObject
            && !source.GetType().IsInstanceOfType(destination))
        {
            throw new ArgumentException(
                $"Cannot set mapping for object of source type {source.GetType()} to destination {destination.GetType()}.",
                nameof(source));
        }
        _sourceDestinationMap[source] = destination;
    }

    /// <summary>
    /// Map object to itself and ignore cloning.
    /// </summary>
    /// <param name="source">Source object.</param>
    public void SetSelfSourceObjectMapping(object source) => SetSourceObjectMapping(source, source);

    /// <summary>
    /// Get destination object by source object.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <returns>Destination object.</returns>
    public object? GetSourceObjectMapping(object source)
    {
        if (TryGetSourceObjectMapping(source, out var destination))
        {
            return destination;
        }
        throw new KeyNotFoundException($"Cannot get mapping for object of source type {source.GetType()}.");
    }

    /// <summary>
    /// Try to get destination mapping.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="result">Result object.</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool TryGetSourceObjectMapping(object source, out object? result)
        => _sourceDestinationMap.TryGetValue(source, out result);

    /// <summary>
    /// Contains in object mapping.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <returns>True if contains.</returns>
    public bool ContainsInObjectMapping(object source) => _sourceDestinationMap.ContainsKey(source);

    /// <summary>
    /// Remove source object mapping.
    /// </summary>
    /// <param name="source">Source object.</param>
    public void RemoveSourceObjectMapping(object source)
    {
        _sourceDestinationMap.Remove(source);
    }

    /// <summary>
    /// Clear objects mappings.
    /// </summary>
    public void ClearMappings()
    {
        _sourceDestinationMap.Clear();
    }

    /// <summary>
    /// Add custom object factory.
    /// </summary>
    /// <param name="type">Object type.</param>
    /// <param name="factory">Function to create object.</param>
    public void AddObjectFactory(Type type, Func<InputInfo, object> factory)
        => _objectFactories.Add(type, factory);

    /// <summary>
    /// Get destination object by source object.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>Destination object.</returns>
    public T GetMapped<T>(T source) where T : class
    {
        var obj = GetSourceObjectMapping(source);
        if (obj == null)
        {
            throw new InvalidOperationException($"Cannot get mapping for object of source type {source.GetType()}.");
        }
        return (T)obj;
    }

    #endregion

    /// <summary>
    /// Execute the specific handler after the specific object was cloned. The actions
    /// are executed when the whole tree of nested properties were cloned. The action is executed only once.
    /// </summary>
    /// <param name="obj">Object to execute action after clone.</param>
    /// <param name="handler">Handler to execute.</param>
    public void AddPostCloneHandler(object obj, Action<object> handler)
    {
        if (_postCloneHandlers.TryGetValue(obj, out var existingHandler))
        {
            // Avoid duplicate handlers.
            var handlerDelegate = handler.GetInvocationList()[0];
            if (Array.IndexOf(existingHandler.GetInvocationList(), handlerDelegate) > -1)
            {
                return;
            }
            _postCloneHandlers[obj] = handler + existingHandler;
        }
        else
        {
            _postCloneHandlers[obj] = handler;
        }
    }

    /// <summary>
    /// Clone object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>Cloned object.</returns>
    public T Clone<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var type = obj.GetType();
        var inputInfo = new InputInfo(obj, type, Depth: _path.Count + 1);
        var cloned = Copy(ref inputInfo)!;
        if (IsIgnoredObject(cloned))
        {
            throw new InvalidOperationException($"Cannot clone object of type {type}.");
        }
        return (T)cloned;
    }

    /// <summary>
    /// Clone field from the source object to the destination.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="destination">Destination object.</param>
    /// <param name="simpleTypesOnly">Copy only simple type fields.</param>
    /// <typeparam name="T">Object type.</typeparam>
    public void CloneTo<T>(T source, T destination, bool simpleTypesOnly = false)
    {
        if (source == null)
        {
            return;
        }
        var inputInfo = new InputInfo(source, source.GetType());
        TryCopyObjectInternal(ref inputInfo, destination, simpleTypesOnly, addToSourceDestinationMap: false);
    }

    private object? Copy(ref InputInfo inputInfo)
    {
        var source = inputInfo.Source;
        if (source == null)
        {
            return null;
        }

        _path.Push(inputInfo);

        object? resultObject = null;
        ClonerDelegate? clonerMethod = null;
        foreach (var cloneMethod in _cloneMethods)
        {
            var result = cloneMethod.Invoke(ref inputInfo);
            if (!result.Handled)
            {
                continue;
            }

            clonerMethod = cloneMethod;
            resultObject = result.Destination;
            if (resultObject != IgnoreObject)
            {
                if (AfterClone != null)
                {
                    resultObject = AfterClone.Invoke(inputInfo, resultObject);
                }

                if (clonerMethod != TryCopySimpleObject
                    && clonerMethod != TryGetMapped
                    && clonerMethod != TryCopySpecialType
                    && !inputInfo.Type.IsValueType)
                {
                    _sourceDestinationMap[source] = resultObject;
                }
            }

            break;
        }

        _path.Pop();

        if (resultObject != null
            && _postCloneHandlers.Count > 0
            && clonerMethod != TryGetMapped
            && !IsSourceInPath(source)
            && _postCloneHandlers.Remove(source, out var handler))
        {
            handler.Invoke(resultObject);
        }

        return resultObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSourceInPath(object source)
    {
        foreach (var pathItem in _path)
        {
            if (object.ReferenceEquals(pathItem.Source, source))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get property info by back field.
    /// </summary>
    /// <param name="fieldInfo">Field info.</param>
    /// <returns>Property info or null.</returns>
    public static PropertyInfo? GetPropertyByField(FieldInfo fieldInfo)
    {
        return _fieldToProperties.GetOrAdd(fieldInfo, field =>
        {
            if (field.ReflectedType == null)
            {
                return null;
            }
            var name = field.Name.AsSpan();

            // Get by backing field C# (AFAIK C# 11) compiler convention.
            ReadOnlySpan<char> propertyName;
            if (name.StartsWith('<') || name.Contains("k__BackingField", StringComparison.Ordinal))
            {
                var start = name.IndexOf('<') + 1;
                var end = name.LastIndexOf('>');
                propertyName = name[start..end];
            }
            // Fallback to standard convention.
            else
            {
                propertyName = name.StartsWith('_') ? name[1..] : name;
            }

            var properties = field.ReflectedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static);
            foreach (var prop in properties)
            {
                if (prop.DeclaringType == field.DeclaringType && prop.Name.Equals(propertyName, StringComparison.Ordinal))
                {
                    return prop;
                }
            }
            return null;
        });
    }

    #region Cache functions

    private static readonly Type[] _typesForExtendedCloneMethod = [typeof(CloneIt)];

    private static Func<object, CloneIt, object>? GetCloneWithClonerMethod(Type type)
    {
        return _extendedCloneMethodTypes.GetOrAdd(type, t =>
        {
            var method = t.GetMethod(nameof(ICloneable.Clone), BindingFlags.Public | BindingFlags.Instance,
                _typesForExtendedCloneMethod);
            if (method == null)
            {
                return null;
            }

#if USE_FAST_EXPRESSION_COMPILER
            var clonerParam = FastExpressionCompiler.LightExpression.Expression.Parameter(typeof(CloneIt), "cloner");
            var objectInstanceExpression = FastExpressionCompiler.LightExpression.Expression.Parameter(typeof(object), "instance");
            var objectTypeInstanceExpression = FastExpressionCompiler.LightExpression.Expression.Convert(objectInstanceExpression, t);
            var callExpr = FastExpressionCompiler.LightExpression.Expression.Call(objectTypeInstanceExpression, method, clonerParam);
            var lambda = FastExpressionCompiler.LightExpression.Expression.Lambda<Func<object, CloneIt, object>>(
                callExpr, objectInstanceExpression, clonerParam);
            return FastExpressionCompiler.LightExpression.ExpressionCompiler.CompileFast(lambda);
#else
            var clonerParam = System.Linq.Expressions.Expression.Parameter(typeof(CloneIt), "cloner");
            var objectInstanceExpression = System.Linq.Expressions.Expression.Parameter(typeof(object), "instance");
            var objectTypeInstanceExpression = System.Linq.Expressions.Expression.Convert(objectInstanceExpression, t);
            var callExpr = System.Linq.Expressions.Expression.Call(objectTypeInstanceExpression, method, clonerParam);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<object, CloneIt, object>>(
                callExpr, objectInstanceExpression, clonerParam);
            return lambda.Compile();
#endif
        });
    }

    private static Func<IEnumerable>? GetMethodToCreateCollection(Type type)
    {
        return _collectionsConstructorTypes.GetOrAdd(type, t =>
        {
#if USE_FAST_EXPRESSION_COMPILER
            var newExpr = FastExpressionCompiler.LightExpression.Expression.New(t);
            var lambda = FastExpressionCompiler.LightExpression.Expression.Lambda<Func<IEnumerable>>(newExpr);
            return FastExpressionCompiler.LightExpression.ExpressionCompiler.CompileFast(lambda);
#else
            var newExpr = System.Linq.Expressions.Expression.New(t);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<IEnumerable>>(newExpr);
            return lambda.Compile();
#endif
        });
    }

    private static MethodInfo? GetAddCollectionMethod(Type type)
    {
        return _collectionsAddMethods.GetOrAdd(type, t => t.GetMethod("Add"));
    }

    private static FieldInfo[] GetAllFields(Type type)
    {
        return _typeClonableFields.GetOrAdd(type, t =>
        {
            var list = new List<FieldInfo>(32);
            var baseType = t;
            while (baseType != null)
            {
                var fields = baseType.GetFields(BindingFlags.Instance |
                                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsStatic || field.IsLiteral)
                    {
                        continue;
                    }
                    list.Add(field);
                }
                baseType = baseType.BaseType;
            }
            return list.ToArray();
        });
    }

    #endregion

    #region Handlers

    private ResultInfo TryGetIsIgnored(ref InputInfo inputInfo)
    {
        if (inputInfo.Depth > MaxDepth)
        {
            return GetIgnoreResultInfo();
        }
        if (inputInfo.FieldInfo != null && IgnoreFieldFunc?.Invoke(inputInfo.FieldInfo) == true)
        {
            return GetIgnoreResultInfo();
        }
        if (inputInfo.Source != null && IgnoreObjectFunc?.Invoke(inputInfo.Source) == true)
        {
            return GetIgnoreResultInfo();
        }
        foreach (Type ignoreType in IgnoreTypes)
        {
            if (ignoreType == inputInfo.Type
                || (ignoreType.IsInterface && ignoreType.IsAssignableFrom(inputInfo.Type)))
            {
                return GetIgnoreResultInfo();
            }
        }
        return GetNoResultInfo();
    }

    private ResultInfo TryCopyWithCustomCloner(ref InputInfo inputInfo)
    {
        if (CustomCloner == null)
        {
            return GetNoResultInfo();
        }

        return CustomCloner.Invoke(ref inputInfo);
    }

    private ResultInfo TryGetMapped(ref InputInfo inputInfo)
    {
        if (inputInfo.Source != null && _sourceDestinationMap.TryGetValue(inputInfo.Source, out object? result))
        {
            return GetOkResultInfo(result);
        }
        return GetNoResultInfo();
    }

    private ResultInfo TryGetCloneable(ref InputInfo inputInfo)
    {
        if (!UseIClonable)
        {
            return GetNoResultInfo();
        }
        if (inputInfo.Source == null)
        {
            return GetOkResultInfo();
        }

        // Avoid recursion.
        foreach (var item in _clonableIgnoreObjects)
        {
            if (ReferenceEquals(item, inputInfo.Source))
            {
                return GetNoResultInfo();
            }
        }

        // If we faced with self-recursion, let's skip it.
        if (_selfRecursionPath.Contains(inputInfo.Source))
        {
#if DEBUG
            var recursionPath = string.Join(" -> ", _selfRecursionPath);
            System.Diagnostics.Debug.WriteLine("Clone recursion: " + recursionPath);
#endif
            return GetNoResultInfo();
        }

        var cloneFunc = GetCloneWithClonerMethod(inputInfo.Type);
        if (cloneFunc != null)
        {
            _selfRecursionPath.Add(inputInfo.Source);
            var clone = cloneFunc.Invoke(inputInfo.Source, this);
            FinishRecursionObjectProcessing(inputInfo.Source, clone);
            return GetOkResultInfo(clone);
        }

        if (inputInfo.Source is ICloneable cloneable)
        {
            _selfRecursionPath.Add(inputInfo.Source);
            var clone = cloneable.Clone();
            FinishRecursionObjectProcessing(inputInfo.Source, clone);
            return GetOkResultInfo(clone);
        }

        void FinishRecursionObjectProcessing(in object sourceObject, object? targetObject)
        {
            _selfRecursionPath.Remove(sourceObject);
            _sourceDestinationMap[sourceObject] = targetObject;
        }

        return GetNoResultInfo();
    }

    private ResultInfo TryCopyObjectFactory(ref InputInfo inputInfo)
    {
        if (_objectFactories.TryGetValue(inputInfo.Type, out var func))
        {
            return GetOkResultInfo(func.Invoke(inputInfo));
        }
        return GetNoResultInfo();
    }

    private ResultInfo TryCopySimpleObject(ref InputInfo inputInfo)
    {
        if (!IsSimpleType(inputInfo.Type) || inputInfo.Source == null)
        {
            return GetNoResultInfo();
        }
        return GetOkResultInfo(inputInfo.Source);
    }

    private ResultInfo TryCopyWithCopyConstructor(ref InputInfo inputInfo)
    {
        if (!UseCopyConstructor || inputInfo.Source == null)
        {
            return GetNoResultInfo();
        }

        var constructor = inputInfo.Type.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [inputInfo.Type, typeof(CloneIt)]);
        if (constructor == null)
        {
            return GetNoResultInfo();
        }
        var emptyObject = CreateEmptyObject(inputInfo.Type);
        _sourceDestinationMap.Add(inputInfo.Source, emptyObject);
        constructor.Invoke(emptyObject, [inputInfo.Source, this]);
        return GetOkResultInfo(emptyObject);
    }

    private ResultInfo TryCopyObject(ref InputInfo inputInfo)
    {
        return TryCopyObjectInternal(ref inputInfo);
    }

    private ResultInfo TryCopyObjectInternal(
        ref InputInfo inputInfo,
        object? source = null,
        bool simpleTypesOnly = false,
        bool addToSourceDestinationMap = true)
    {
        if (inputInfo.Source == null)
        {
            return GetOkResultInfo();
        }

        var cloned = source ?? CreateEmptyObject(inputInfo.Type);

        if (addToSourceDestinationMap)
        {
            _sourceDestinationMap.Add(inputInfo.Source, cloned);
        }

        var allFields = GetAllFields(inputInfo.Type);
        foreach (FieldInfo fieldInfo in allFields)
        {
            if (simpleTypesOnly && !IsSimpleType(fieldInfo.FieldType))
            {
                continue;
            }

            // Copy value types.
            if ((IgnoreFieldFunc != null && IgnoreFieldFunc.Invoke(fieldInfo))
                || (IgnoreObjectFunc != null && IgnoreObjectFunc.Invoke(fieldInfo.GetValue(inputInfo.Source)))
                || (!UsePropertyCopy && TryCopyFast(inputInfo.Source, cloned, fieldInfo)))
            {
                continue;
            }

            var value = fieldInfo.GetValue(inputInfo.Source);
            if (value == null)
            {
                continue;
            }

            var info = new InputInfo(value, value.GetType(), FieldInfo: fieldInfo, Depth: inputInfo.Depth + 1);
            var copied = Copy(ref info);
            if (!IsIgnoredObject(copied))
            {
                if (UsePropertyCopy)
                {
                    var property = GetPropertyByField(fieldInfo);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(cloned, copied);
                    }
                    else
                    {
                        fieldInfo.SetValue(cloned, copied);
                    }
                }
                else
                {
                    fieldInfo.SetValue(cloned, copied);
                }
            }
        }
        return GetOkResultInfo(cloned);
    }

    private ResultInfo TryCopySpecialType(ref InputInfo inputInfo)
    {
        if (!CopyEventHandlers)
        {
            if ((!inputInfo.Type.IsGenericType
                && typeof(EventHandler).IsAssignableFrom(inputInfo.Type))
                ||
                (inputInfo.Type.IsGenericType
                    && typeof(EventHandler<>).IsAssignableFrom(inputInfo.Type.GetGenericTypeDefinition()))
                )
            {
                return GetIgnoreResultInfo();
            }
            if (typeof(PropertyChangedEventHandler).IsAssignableFrom(inputInfo.Type))
            {
                return GetIgnoreResultInfo();
            }
        }

        if (typeof(Delegate).IsAssignableFrom(inputInfo.Type))
        {
            return GetOkResultInfo(inputInfo.Source);
        }

        if (typeof(Type).IsAssignableFrom(inputInfo.Type))
        {
            return GetOkResultInfo(inputInfo.Source);
        }

        return GetNoResultInfo();
    }

    private ResultInfo TryCopyEnumerable(ref InputInfo inputInfo)
    {
        if (inputInfo.Source is not IEnumerable enumerableSource || !inputInfo.Type.IsGenericType)
        {
            return GetNoResultInfo();
        }

        MethodInfo? addMethod = null;
        if (inputInfo.Source is not IList)
        {
            addMethod = GetAddCollectionMethod(inputInfo.Type);
            if (addMethod == null)
            {
                var copy = _cloneMethod.Invoke(inputInfo.Source, null);
                return GetOkResultInfo(copy);
            }
        }

        var newList = GetMethodToCreateCollection(enumerableSource.GetType())!.Invoke();
        foreach (var sourceItem in enumerableSource)
        {
            if (sourceItem == null
                || (IgnoreObjectFunc != null && IgnoreObjectFunc.Invoke(sourceItem)))
            {
                continue;
            }

            if (!TryCopyFast(sourceItem, out var value))
            {
                var info = new InputInfo(sourceItem, sourceItem.GetType(), Depth: inputInfo.Depth + 1);
                value = Copy(ref info);
                if (IsIgnoredObject(value))
                {
                    continue;
                }
            }
            if (newList is IList list)
            {
                list.Add(value);
            }
            else if (addMethod != null)
            {
                addMethod.Invoke(newList, [value]);
            }
        }
        return GetOkResultInfo(newList);
    }

    private ResultInfo TryCopyDictionary(ref InputInfo inputInfo)
    {
        if (inputInfo.Source is not IDictionary dictionary || !inputInfo.Type.IsGenericType)
        {
            return GetNoResultInfo();
        }

        // Create new dictionary.
        var genericType = inputInfo.Type.GetGenericArguments()[1];
        var newDictionary = (IDictionary)GetMethodToCreateCollection(inputInfo.Type)!.Invoke();

        // Fill new dictionary.
        foreach (DictionaryEntry keyValue in dictionary)
        {
            if (IgnoreObjectFunc != null && (IgnoreObjectFunc.Invoke(keyValue.Key)
                || IgnoreObjectFunc.Invoke(keyValue.Value)))
            {
                continue;
            }
            if (!TryCopyFast(keyValue.Key, out var key))
            {
                var inputKey = new InputInfo(keyValue.Key, keyValue.Key.GetType(), Depth: inputInfo.Depth + 1);
                key = Copy(ref inputKey);
            }
            if (key == null)
            {
                continue;
            }
            if (!TryCopyFast(keyValue.Value, out var value))
            {
                var inputValue = new InputInfo(keyValue.Value,
                    keyValue.Value?.GetType() ?? genericType, Depth: inputInfo.Depth + 1);
                value = Copy(ref inputValue);
                if (IsIgnoredObject(value))
                {
                    continue;
                }
            }
            newDictionary.Add(key, value);
        }

        return GetOkResultInfo(newDictionary);
    }

    private ResultInfo TryCopyArray(ref InputInfo inputInfo)
    {
        if (inputInfo.Source is not Array arraySource || !inputInfo.Type.IsArray)
        {
            return GetNoResultInfo();
        }

        var arrayDest = CreateCopyArrayInstance(arraySource, inputInfo.Type);

        if (inputInfo.Type.GetElementType()?.IsValueType == true)
        {
            Array.Copy(arraySource, arrayDest, arraySource.Length);
        }
        else
        {
            var arrayValueIndices = GetArrayValueIndices(arraySource, new int[arraySource.Rank]);
            foreach (var valueIndices in arrayValueIndices)
            {
                var value = arraySource.GetValue(valueIndices);
                if (value == null)
                {
                    continue;
                }
                if (!TryCopyFast(value, out var copied))
                {
                    var info = new InputInfo(value, value.GetType(), Depth: inputInfo.Depth + 1);
                    copied = Copy(ref info);
                }
                if (copied != IgnoreObject)
                {
                    arrayDest.SetValue(copied, valueIndices);
                }
            }
        }
        return GetOkResultInfo(arrayDest);
    }

    #endregion

    /// <summary>
    /// Get array values indices via recursion.
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="indices">Indices of array value.
    /// ([3] -> get value in 3 cell 1D array; [2, 3] -> get value in 2 row, 3 cell 2D array, and etc.).</param>
    /// <param name="rank">Rank.</param>
    /// <remarks>Logic is similar to preorder traversal tree.</remarks>
    /// <returns>Values indices.</returns>
    private static IEnumerable<int[]> GetArrayValueIndices(Array array, int[] indices, int rank = 0)
    {
        for (var i = 0; i < array.GetLength(rank); i++)
        {
            indices[rank] = i;
            var nextRank = rank + 1;

            if (array.Rank == nextRank)
            {
                yield return indices;
            }
            else
            {
                foreach (var valueIndex in GetArrayValueIndices(array, indices, nextRank))
                {
                    yield return valueIndex;
                }
            }
        }
    }

    private static Array CreateCopyArrayInstance(Array arraySource, Type arrayType)
    {
        var constructorParam = new int[arraySource.Rank];

        // Save length of dimensions for constructor
        for (var rank = 0; rank < arraySource.Rank; rank++)
        {
            constructorParam[rank] = arraySource.GetLength(rank);
        }

        var elementType = arrayType.GetElementType()!;
        var arrayInstance = Array.CreateInstance(elementType, constructorParam);

        return arrayInstance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCopyFast<T>(T source, out T? target)
    {
        if (source == null)
        {
            target = default;
            return true;
        }
        var type = source.GetType();
        if (type.IsValueType)
        {
            target = source;
            return true;
        }
        if (type == typeof(string))
        {
            target = source;
            return true;
        }
        target = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCopyFast(object source, object target, FieldInfo fieldInfo)
    {
        if (fieldInfo.FieldType.IsValueType)
        {
            TypedReference typedReference = __makeref(source);
            var rf = fieldInfo.GetValueDirect(typedReference);
            if (rf != null)
            {
                var clonedObj = __makeref(target);
                fieldInfo = GetCurrentAssemblyContextFieldType(fieldInfo, target);
                fieldInfo.SetValueDirect(clonedObj, rf);
                return true;
            }
        }
        if (fieldInfo.FieldType == typeof(string))
        {
            var str = fieldInfo.GetValue(source);
            fieldInfo = GetCurrentAssemblyContextFieldType(fieldInfo, target);
            fieldInfo.SetValue(target, str);
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FieldInfo GetCurrentAssemblyContextFieldType(FieldInfo fieldInfo, object target)
    {
        var sourceType = target.GetType();
        if (fieldInfo.DeclaringType == sourceType)
        {
            return fieldInfo;
        }

        // Sometime type is loaded into another AssemblyLoadContext and we cannot set value to field from another context,
        // so we need to get field from source type.
        var newFieldInfo = sourceType.GetField(fieldInfo.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (newFieldInfo == null || newFieldInfo.FieldType != fieldInfo.FieldType)
        {
            return fieldInfo;
        }

        return newFieldInfo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object CreateEmptyObject(Type type)
    {
        return RuntimeHelpers.GetUninitializedObject(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIgnoredObject<T>(T obj)
    {
        if (obj == null || obj.GetType().IsValueType)
        {
            return false;
        }
        return ReferenceEquals(obj, IgnoreObject);
    }

    #region Simple types resolver

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
            || type.IsValueType
            || type == typeof(string)
            || Array.IndexOf(_simpleTypes, type) > -1
            || type.IsEnum
            || Convert.GetTypeCode(type) != TypeCode.Object
            || (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && IsSimpleType(type.GetGenericArguments()[0]));
    }

    private static readonly Type[] _simpleTypes =
    [
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid)
    ];

    #endregion

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        /// <inheritdoc />
        bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);

        /// <inheritdoc />
        int IEqualityComparer<object>.GetHashCode(object? obj) => RuntimeHelpers.GetHashCode(obj);
    }

    #region Dispose

    /// <summary>
    /// Dispose pattern.
    /// </summary>
    /// <param name="disposing">Dispose manager resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var postCloneHandler in _postCloneHandlers)
            {
                postCloneHandler.Value.Invoke(postCloneHandler.Key);
            }
            _postCloneHandlers.Clear();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
