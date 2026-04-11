namespace QSM.Core.ModPluginSource;

public class ModPluginInfo
{
	public object? Id { get; init; }
	public string Description { get; init; } = string.Empty;
	public ulong DownloadCount { get; init; }
	public string? IconUrl { get; init; }
	public required string License { get; init; }
	public string LicenseUrl { get; set; } = string.Empty;
	public string LongDescription { get; set; } = string.Empty;
	public required string Name { get; init; }
	public required string Owner { get; init; }
	public required string Slug { get; init; }
	
	/// <summary>
	/// URL to the mod's page
	/// </summary>
	public string? Url { get; set; }
}