namespace QSM.Core.ServerSoftware;

/// <summary>
///     Contains the basic server information.
/// </summary>
public class ServerMetadata
{
	public static ServerMetadata? Selected = null;

	public static readonly ServerSoftwares[] SupportMods =
	[
		ServerSoftwares.Fabric,
		ServerSoftwares.NeoForge
	];

	public static readonly ServerSoftwares[] SupportPlugins =
	[
		ServerSoftwares.Paper,
		ServerSoftwares.Purpur,
		ServerSoftwares.Velocity
	];

	public ServerMetadata() { }

	public ServerMetadata(string name, ServerSoftwares software, string minecraftVersion, string serverVersion,
		string serverPath, Guid? guid = null)
	{
		Name = name;
		Software = software;
		MinecraftVersion = minecraftVersion;
		ServerVersion = serverVersion;
		ServerPath = serverPath;
		Guid = guid ?? Guid.NewGuid();
	}

	public Guid Guid { get; set; }
	public string Name { get; set; } = "";
	public ServerSoftwares Software { get; init; }
	public string MinecraftVersion { get; init; } = "";
	public string ServerVersion { get; init; } = "";
	public string ServerPath { get; set; } = "";

	public bool IsModSupported => SupportMods.Contains(Software);

	public bool IsPluginSupported => SupportPlugins.Contains(Software);

	public string QsmConfigFile => Path.Combine(ServerPath, "config.qsm.json");
}