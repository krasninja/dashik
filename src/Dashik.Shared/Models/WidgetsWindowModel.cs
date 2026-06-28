using System.Runtime.Serialization;
using Dashik.Sdk.Utils;

namespace Dashik.Shared.Models;

[DataContract]
public sealed class WidgetsWindowModel
{
    [DataMember]
    public string Id { get; set; } = IdGenerator.Generate();

    [DataMember]
    public string SelectedSpace { get; set; } = string.Empty;

    [DataMember]
    public WidgetsWindowType Type { get; set; } = WidgetsWindowType.Window;
}
