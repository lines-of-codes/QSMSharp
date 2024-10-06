namespace QSM.Core.ServerSettings;

/// <summary>
/// Contains a more in-depth information of a server.
/// </summary>
public partial class ServerSettings
{
	public List<BackupItem> Backups { get; set; } = [];

	public JavaSettings Java { get; set; } = new();
}
