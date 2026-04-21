using JetBrains.Annotations;
using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

public class VanillaFetcher(IHttpClientFactory factory) : InfoFetcher
{
	public override string HttpClientName => "VanillaFetcher";
	public override string HttpBaseAddress => "https://piston-meta.mojang.com/mc/game/";

	private IEnumerable<VersionEntry> _versionCache = [];

	public override Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		return Task.FromResult<string[]>([""]);
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (MinecraftVersionsCache.Length != 0)
		{
			return MinecraftVersionsCache;
		}

		HttpClient client = factory.CreateClient(HttpClientName);
		VersionManifest response = await client.GetFromJsonAsync<VersionManifest>("version_manifest_v2.json")
								   ?? throw new NetworkResourceUnavailableException();

		_versionCache = response.Versions.Where(e => e.Type == "release");
		MinecraftVersionsCache = _versionCache.Select(e => e.Id).ToArray();

		return MinecraftVersionsCache;
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		string versionUrl = _versionCache.First(e => e.Id == minecraftVersion).Url;
		HttpClient client = factory.CreateClient(HttpClientName);
		VersionInfo response = await client.GetFromJsonAsync<VersionInfo>(versionUrl)
							   ?? throw new NetworkResourceUnavailableException();

		return response.Downloads.Server.Url;
	}

	/// <summary>
	/// Latest releases. Included for the sake of completeness
	/// </summary>
	/// <param name="Release">Latest stable release</param>
	/// <param name="Snapshot">Latest snapshot release</param>
	[UsedImplicitly]
	private sealed record Latest(string Release, string Snapshot);

	[UsedImplicitly]
	private sealed record VersionEntry(
		string Id,
		string Type,
		string Url,
		string Time,
		string ReleaseTime,
		string Sha1,
		byte ComplianceLevel);

	private sealed record VersionManifest(
		Latest Latest,
		VersionEntry[] Versions);

	[UsedImplicitly]
	private sealed record VersionDownloadEntry(
		string Sha1,
		int Size,
		string Url);

	[UsedImplicitly]
	private sealed record VersionDownloads(
		VersionDownloadEntry Client,
		VersionDownloadEntry Server);

	private sealed record VersionInfo(
		VersionDownloads Downloads);
}