using System.Text.Json.Serialization;

namespace QSM.Windows;

[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(ApplicationConfiguration))]
internal partial class ApplicationConfigurationContext : JsonSerializerContext
{
}
