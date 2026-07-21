using ReactiveUI;
using YamlDotNet.Serialization;
using Dashik.Host.Utils;
using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

public class YamlSectionViewModel : SettingsSectionModel
{
    private static readonly SimpleObjectTraverser _traverser = new();

    /// <inheritdoc />
    public override object? Settings
    {
        get;
        set
        {
            RebuildSerializers(value);
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public string YamlError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public event EventHandler? Sync;

    private static readonly ISerializer _defaultSerializer = new SerializerBuilder()
        .Build();

    internal ISerializer Serializer { get; private set; } = _defaultSerializer;

    private static readonly IDeserializer _defaultDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    internal IDeserializer Deserializer { get; private set; } = _defaultDeserializer;

    /// <inheritdoc />
    public YamlSectionViewModel()
    {
        YamlError = string.Empty;
    }

    private void RebuildSerializers(object? obj)
    {
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

        var serializerBuilder = new SerializerBuilder()
            .DisableAliases();
        foreach (var type in types)
        {
            serializerBuilder = serializerBuilder.WithTagMapping(FormatTypeTag(type), type);
        }
        Serializer = serializerBuilder.EnsureRoundtrip().Build();

        var deserializerBuilder = new DeserializerBuilder()
            .IgnoreUnmatchedProperties();
        foreach (var type in types)
        {
            deserializerBuilder = deserializerBuilder.WithTagMapping(FormatTypeTag(type), type);
        }
        Deserializer = deserializerBuilder.Build();
    }

    /// <inheritdoc />
    public override void SyncSetting()
    {
        Sync?.Invoke(this, EventArgs.Empty);
        base.SyncSetting();
    }
}
