using QSM.Core.ServerSoftware;
using System.Text.Json.Serialization;

namespace QSM.Core.ModPluginSource.Modrinth;

/// <summary>
/// The structure of the modrinth.index.json file according to 
/// https://support.modrinth.com/en/articles/8802351-modrinth-modpack-format-mrpack
/// </summary>
public class MrpackModrinthIndex
{
	[JsonPropertyName("formatVersion")]
	public int FormatVersion { get; set; }

	[JsonPropertyName("game")]
	public string Game { get; set; } = "minecraft";

	[JsonPropertyName("versionId")]
	public string VersionId { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	[JsonPropertyName("files")]
	public MrpackFile[] Files { get; set; } = [];

	[JsonPropertyName("dependencies")]
	public Dictionary<string, string> Dependencies { get; set; } = [];

	private KeyValuePair<string, string> SoftwareKeyPair => Dependencies.First(pair => pair.Key != "minecraft");

	public string MinecraftVersion => Dependencies["minecraft"];
	public string MinecraftSoftwareVersion => SoftwareKeyPair.Value;
	public ServerSoftwares MinecraftServerSoftware => SoftwareKeyPair.Key switch
	{
		"neoforge" => ServerSoftwares.NeoForge,
		"fabric-loader" => ServerSoftwares.Fabric,
		_ => throw new InvalidOperationException(),
	};
}
