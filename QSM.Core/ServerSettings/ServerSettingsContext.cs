using System.Text.Json.Serialization;

namespace QSM.Core.ServerSettings;

[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(ServerSettings))]
internal partial class ServerSettingsContext : JsonSerializerContext
{
}