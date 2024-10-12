namespace QSM.Core.ServerSoftware;

/// <summary>
/// Contains the basic server information.
/// </summary>
public class ServerMetadata
{
    static ServerSoftwares[] SupportMods =
    [
        ServerSoftwares.Fabric,
        ServerSoftwares.NeoForge
    ];

    static ServerSoftwares[] SupportPlugins =
    [
        ServerSoftwares.Paper,
        ServerSoftwares.Purpur,
        ServerSoftwares.Velocity
    ];

    public Guid Guid { get; set; }
    public string Name { get; set; } = "";
    public ServerSoftwares Software { get; init; }
    public string MinecraftVersion { get; init; } = "";
    public string ServerVersion { get; init; } = "";
    public string ServerPath { get; set; } = "";

    public bool IsModSupported
    {
        get => SupportMods.Contains(Software);
    }

    public bool IsPluginSupported
    {
        get => SupportPlugins.Contains(Software);
    }

    public string QsmConfigFile
    {
        get => Path.Combine(ServerPath, "config.qsm.json");
    }

    public ServerMetadata() {}

    public ServerMetadata(string name, ServerSoftwares software, string minecraftVersion, string serverVersion, string serverPath, Guid? guid = null)
    {
        Name = name;
        Software = software;
        MinecraftVersion = minecraftVersion;
        ServerVersion = serverVersion;
        ServerPath = serverPath;
        Guid = guid ?? Guid.NewGuid();
    }
}
