using QSM.Core.ServerSoftware;

namespace QSM.Core.ModPluginSource;

// skipcq: CS-R1078
// False positive
public abstract class ModPluginProvider
{
	public abstract Task<ModPluginInfo[]> SearchAsync(string query = "", ServerMetadata? serverMetadata = null);

	public abstract Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin);

	/// <summary>
	///     Get available versions for
	/// </summary>
	/// <param name="slug">The project's slug</param>
	/// <param name="serverMetadata"></param>
	/// <returns>Returns a dictionary in a Version:DownloadUrl format</returns>
	public abstract Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug, ServerMetadata? serverMetadata = null);

	public abstract Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod);

	/// <summary>
	///     Check for mod/plugin updates.
	///     This function is on-hold, will implement later. (But contributions are still welcome)
	/// </summary>
	/// <param name="modFiles">The path to the mod files.</param>
	/// <returns>The download information for available updates.</returns>
	public abstract Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles);
}