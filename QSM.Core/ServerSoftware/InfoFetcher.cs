using QSM.Core.Utilities;

namespace QSM.Core.ServerSoftware;

public abstract class InfoFetcher : IHttpConsumer
{
	protected readonly Dictionary<string, string[]> BuildInfoCache = [];
	protected string[] MinecraftVersionsCache = [];

	public virtual string FirstRunArgs => "";

	public abstract string HttpClientName { get; }
	public abstract string HttpBaseAddress { get; }

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