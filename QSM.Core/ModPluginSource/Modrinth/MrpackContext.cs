using System.Text.Json.Serialization;

namespace QSM.Core.ModPluginSource.Modrinth;

[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(MrpackModrinthIndex))]
public partial class MrpackContext : JsonSerializerContext
{
}
