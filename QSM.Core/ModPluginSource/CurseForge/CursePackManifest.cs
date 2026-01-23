using QSM.Core.ServerSoftware;
using System.Text.Json.Serialization;

namespace QSM.Core.ModPluginSource.CurseForge;

public class CursePackManifest
{
	[JsonPropertyName("minecraft")] public MinecraftInfo Minecraft { get; set; }
	[JsonPropertyName("manifestType")] public string ManifestType { get; set; } = "minecraftModpack";
	[JsonPropertyName("manifestVersion")] public byte ManifestVersion { get; set; } = 1;
	[JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
	[JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
	[JsonPropertyName("author")] public string Author { get; set; } = string.Empty;
	[JsonPropertyName("files")] public File[] Files { get; set; } = [];
	[JsonPropertyName("overrides")] public string Overrides { get; set; } = "overrides";

	public struct MinecraftInfo
	{
		[JsonPropertyName("version")] public string Version { get; set; }
		[JsonPropertyName("modLoaders")] public ModLoader[] ModLoaders { get; set; }

		public ModLoader PrimaryLoader => ModLoaders.First(m => m.Primary);
	}
	
	public struct ModLoader
	{
		[JsonPropertyName("id")] public string Id { get; set; }
		[JsonPropertyName("primary")] public bool Primary { get; set; }

		public ServerSoftwares Software => Id.Split('-').First() switch
		{
			"fabric" => ServerSoftwares.Fabric,
			"forge" => ServerSoftwares.Forge,
			"neoforge" => ServerSoftwares.NeoForge,
			_ => throw new ArgumentOutOfRangeException()
		};

		public string Version => Id.Split('-').Last();
	}

	public struct File()
	{
		[JsonPropertyName("projectID")] public uint ProjectId { get; set; }
		[JsonPropertyName("fileID")] public uint FileId { get; set; }
		[JsonPropertyName("required")] public bool Required { get; set; } = true;
	}
}
