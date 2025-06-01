namespace QSM.Core.ModPluginSource;

public class ModPluginInfo
{
	public string Description = string.Empty;
	public uint DownloadCount;
	public string? IconUrl = null;
	public required string License;
	public string LicenseUrl = string.Empty;
	public string LongDescription = string.Empty;
	public required string Name;
	public required string Owner;
	public required string Slug;
}