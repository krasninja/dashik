using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ReactiveUI;
using YamlDotNet.Serialization;
using Dashik.Sdk.Utils;

namespace Dashik.Host.Models;

/// <summary>
/// Space with the container for widgets.
/// </summary>
[DataContract]
public sealed class SpaceModel : ReactiveObject, ICloneable
{
    private const string DefaultId = "DEFAULT";

    public static SpaceModel DefaultInstance => new()
    {
        Id = DefaultId,
        Name = "Main",
    };

    public string Id { get; set; } = IdGenerator.Generate(length: 12);

    public string Name
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = "New Space";

    [JsonIgnore]
    [YamlIgnore]
    public bool Default => string.IsNullOrEmpty(Id) || Id == DefaultId;

    public SpaceModel()
    {
    }

    public SpaceModel(SpaceModel model)
    {
        Id = model.Id;
        Name = model.Name;
    }

    /// <inheritdoc />
    public object Clone() => new SpaceModel(this);

    /// <inheritdoc />
    public override string ToString() => $"{Id}: {Name}";
}
