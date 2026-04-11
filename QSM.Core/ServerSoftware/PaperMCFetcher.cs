using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

/// <summary>
///     An information fetcher that can fetch PaperMC's project (Paper and Velocity)
/// </summary>
public class PaperMCFetcher(string project, IHttpClientFactory factory) : InfoFetcher
{
	public override string HttpClientName => $"PaperMC{project}";
	public override string HttpBaseAddress => $"https://fill.papermc.io/v3/projects/{project}/";

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (BuildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		using HttpClient client = factory.CreateClient(HttpClientName);
		AvailableBuildsRequest? response =
			await client.GetFromJsonAsync<AvailableBuildsRequest>($"versions/{minecraftVersion}");

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

		using HttpClient client = factory.CreateClient(HttpClientName);
		ProjectInformation? response = await client.GetFromJsonAsync<ProjectInformation>($"/v3/projects/{project}");

		if (response == null)
			throw new NetworkResourceUnavailableException();

		var versionList = response.Versions!.Select(pair => pair.Value).SelectMany(arr => arr);

		MinecraftVersionsCache = versionList.ToArray();

		return MinecraftVersionsCache;
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		using HttpClient client = factory.CreateClient(HttpClientName);
		BuildInfo? response = await client.GetFromJsonAsync<BuildInfo>($"versions/{minecraftVersion}/builds/{build}");

		if (response == null)
			throw new NetworkResourceUnavailableException();

		return response.Downloads?.First().Value.Url ?? throw new NetworkResourceUnavailableException();
	}

	internal sealed record AvailableBuildsRequest(
		int[]? Builds = null);

	internal sealed record ProjectInformation(
		Dictionary<string, string[]>? Versions);

	internal sealed record BuildInfo(Dictionary<string, BuildFileInfo>? Downloads);

	internal sealed record BuildFileInfo(
		string? Name,
		Dictionary<string, string>? Checksums,
		int? Size,
		string? Url);
}