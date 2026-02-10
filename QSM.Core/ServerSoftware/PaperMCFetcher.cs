using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

/// <summary>
///     An information fetcher that can fetch PaperMC's project (Paper and Velocity)
/// </summary>
public class PaperMCFetcher : InfoFetcher
{
	private readonly HttpClient _httpClient;

	private readonly string _prefixPath;

	public PaperMCFetcher(string project)
	{
		_httpClient = new HttpClient { BaseAddress = new Uri("https://fill.papermc.io/") };
		_prefixPath = $"/v3/projects/{project}";
	}

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (BuildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		AvailableBuildsRequest? response =
			await _httpClient.GetFromJsonAsync<AvailableBuildsRequest>($"{_prefixPath}/versions/{minecraftVersion}");

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		string[] builds = Array.ConvertAll(response.Builds!, num => num.ToString());

		BuildInfoCache[minecraftVersion] = builds;

		return builds;
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (MinecraftVersionsCache.Length != 0)
		{
			return MinecraftVersionsCache;
		}

		ProjectInformation? response = await _httpClient.GetFromJsonAsync<ProjectInformation>(_prefixPath);

		if (response == null)
			throw new NetworkResourceUnavailableException();

		var versionList = response.Versions!.Select(pair => pair.Value).SelectMany(arr => arr);

		MinecraftVersionsCache = versionList.ToArray();

		return MinecraftVersionsCache;
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		BuildInfo? response = await _httpClient.GetFromJsonAsync<BuildInfo>($"{_prefixPath}/versions/{minecraftVersion}/builds/{build}");

		if (response == null)
			throw new NetworkResourceUnavailableException();

		return response.Downloads?.First().Value.Url ?? throw new NetworkResourceUnavailableException();
	}

	internal record AvailableBuildsRequest(
		int[]? Builds = null);

	internal record ProjectInformation(
		Dictionary<string, string[]>? Versions);

	internal record BuildInfo(Dictionary<string, BuildFileInfo>? Downloads);

	internal record BuildFileInfo(
		string? Name,
		Dictionary<string, string>? Checksums,
		int? Size,
		string? Url);
}