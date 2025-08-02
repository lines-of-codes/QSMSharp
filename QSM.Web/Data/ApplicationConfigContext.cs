using System.Text.Json.Serialization;

namespace QSM.Web.Data;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApplicationConfig))]
public partial class ApplicationConfigContext : JsonSerializerContext
{
	
}