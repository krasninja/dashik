using System.Runtime.Serialization;
using ReactiveUI;
using System.Text.Json.Serialization;
using Dashik.Sdk.Utils;

namespace Dashik.Shared.Models;

/// <summary>
/// Space with the container for widgets.
/// </summary>
[DataContract]
public sealed class SpaceModel : ReactiveObject
{
    private const string DefaultId = "DEFAULT";

    public static SpaceModel DefaultInstance => new()
    {
        Id = DefaultId,
        Name = "Main",
    };

    [DataMember]
    public string Id { get; set; } = IdGenerator.Generate(length: 12);

    [DataMember]
    public string Name
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = "New Space";

    [JsonIgnore]
    public bool Default => string.IsNullOrEmpty(Id) || Id == DefaultId;

    /// <inheritdoc />
    public override string ToString() => $"{Id}: {Name}";
}
