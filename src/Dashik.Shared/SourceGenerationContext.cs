using System.Text.Json.Serialization;
using Dashik.Shared.Models;

namespace Dashik.Shared;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(PackageFeedModel))]
[JsonSerializable(typeof(MainWindowStateModel))]
[JsonSerializable(typeof(MainWindowStateModel.WindowPosition))]
[JsonSerializable(typeof(Dictionary<string, MainWindowStateModel.WindowPosition>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(SpaceModel))]
internal partial class SourceGenerationContext : JsonSerializerContext;
