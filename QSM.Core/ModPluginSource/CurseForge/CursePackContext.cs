using System.Text.Json.Serialization;

namespace QSM.Core.ModPluginSource.CurseForge;

[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CursePackManifest))]
public partial class CursePackContext : JsonSerializerContext
{
	
}