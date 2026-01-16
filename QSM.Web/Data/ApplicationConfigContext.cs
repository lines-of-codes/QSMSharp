using System.Text.Json.Serialization;

namespace QSM.Web.Data;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(ApplicationConfig))]
public partial class ApplicationConfigContext : JsonSerializerContext
{
	
}