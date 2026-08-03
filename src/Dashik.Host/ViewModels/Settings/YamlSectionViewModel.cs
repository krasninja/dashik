using System.Collections.Frozen;
using ReactiveUI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using Dashik.Host.Utils;
using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

public class YamlSectionViewModel : SettingsSectionModel
{
    private static readonly SimpleObjectTraverser _traverser = new();

    /// <summary>
    /// Ignore system-type properties.
    /// </summary>
    private sealed class IgnorePropertiesInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _inspector;

        private static readonly FrozenSet<string> _propertiesToIgnore = new HashSet<string>([
            "PropertyChanged",
            "PropertyChanging",

            // ReactiveObject.
            "Changing",
            "Changed",
            "ThrownExceptions",
        ]).ToFrozenSet();

        public IgnorePropertiesInspector(ITypeInspector inspector)
        {
            _inspector = inspector;
        }

        /// <inheritdoc />
        public override string GetEnumName(Type enumType, string name) => _inspector.GetEnumName(enumType, name);

        /// <inheritdoc />
        public override string GetEnumValue(object enumValue) => _inspector.GetEnumValue(enumValue);

        /// <inheritdoc />
        public override bool HasParseMethod(Type type) => _inspector.HasParseMethod(type);

        /// <inheritdoc />
        public override object? Parse(string value, Type expectedType) => _inspector.Parse(value, expectedType);

        /// <inheritdoc />
        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
        {
            var properties = _inspector.GetProperties(type, container);
            return properties
                .Where(p => !_propertiesToIgnore.Contains(p.Name) && p.CanWrite);
        }
    }

    /// <inheritdoc />
    public override object? Settings
    {
        get
        {
            PullSettings?.Invoke(this, EventArgs.Empty);
            return base.Settings;
        }

        set
        {
            base.Settings = value;
            RebuildSerializers(value);
            PushSettings?.Invoke(this, EventArgs.Empty);
        }
    }

    public string YamlError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Update settings FROM control.
    /// </summary>
    public event EventHandler? PullSettings;

    /// <summary>
    /// Update settings TO control.
    /// </summary>
    public event EventHandler? PushSettings;

    private static readonly ISerializer _defaultSerializer = new SerializerBuilder()
        .Build();

    internal ISerializer Serializer { get; private set; } = _defaultSerializer;

    private static readonly IDeserializer _defaultDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    internal IDeserializer Deserializer { get; private set; } = _defaultDeserializer;

    private Type? _settingsObjectType;

    /// <inheritdoc />
    public YamlSectionViewModel()
    {
        YamlError = string.Empty;
    }

    private void RebuildSerializers(object? obj)
    {
        if (obj == null)
        {
            return;
        }
        if (_settingsObjectType == obj.GetType())
        {
            return;
        }

        var types = new HashSet<Type>();

        static string FormatTypeTag(Type type) => $"!type-{type.Name}";

        // Collect all types which property type is not equal to object type.
        _traverser.Traverse(obj, (in info) =>
        {
            if (info.Object == null
                || SimpleObjectTraverser.IsSimpleType(info.Object.GetType())
                || info.Tag == null)
            {
                return true;
            }

            var type = info.Object.GetType();
            if (info.PropertyInfo.PropertyType == type)
            {
                return true;
            }

            var list = (HashSet<Type>)info.Tag;
            list.Add(type);
            return true;
        }, types);

        var serializerBuilder = new SerializerBuilder();
        foreach (var type in types)
        {
            serializerBuilder = serializerBuilder.WithTagMapping(FormatTypeTag(type), type);
        }
        Serializer = serializerBuilder
            .DisableAliases()
            .EnsureRoundtrip()
            .WithTypeInspector(typeInspector => new IgnorePropertiesInspector(typeInspector), w => w.OnBottom())
            .Build();

        var deserializerBuilder = new DeserializerBuilder();
        foreach (var type in types)
        {
            deserializerBuilder = deserializerBuilder.WithTagMapping(FormatTypeTag(type), type);
        }
        Deserializer = deserializerBuilder
            .IgnoreUnmatchedProperties()
            .WithTypeInspector(typeInspector => new IgnorePropertiesInspector(typeInspector))
            .Build();

        _settingsObjectType = obj.GetType();
    }
}
