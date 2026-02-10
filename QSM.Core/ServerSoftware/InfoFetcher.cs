namespace QSM.Core.ServerSoftware;

public abstract class InfoFetcher
{
	protected readonly Dictionary<string, string[]> BuildInfoCache = [];
	protected string[] MinecraftVersionsCache = [];

	public virtual string HttpClientName => "InfoFetcher";
	public virtual string FirstRunArgs => "";
	public virtual string HttpBaseAddress => "";

	public abstract Task<string[]> FetchAvailableMinecraftVersionsAsync();

	/// <summary>
	///     A function to fetch available software builds for a specific version of Minecraft.
	/// </summary>
	/// <returns>
	///     A dictionary where the keys are the build number and the values are the download link to the build
	/// </returns>
	public abstract Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion);

	public abstract Task<string> GetDownloadUrlAsync(string minecraftVersion, string build);
}