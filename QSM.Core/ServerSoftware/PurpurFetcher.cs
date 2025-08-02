using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

public class PurpurFetcher : InfoFetcher
{
	private const string prefixPath = "/v2/purpur/";
	private readonly HttpClient httpClient;

	public PurpurFetcher()
	{
		httpClient = new HttpClient { BaseAddress = new Uri("https://api.purpurmc.org/") };
	}

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (buildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		BuildInfoRequest? response = await httpClient.GetFromJsonAsync<BuildInfoRequest>(prefixPath + minecraftVersion);

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		string[] builds = response.Builds!.All!;

		Array.Sort(builds, (a, b) =>
		{
			// Ignore the result, as the output integer will be 0 if it failed anyway.
			_ = int.TryParse(a, out int aint);
			_ = int.TryParse(b, out int bint);

			return aint - bint;
		});
		Array.Reverse(builds);

		buildInfoCache[minecraftVersion] = builds;

		return builds;
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (minecraftVersionsCache.Length != 0)
		{
			return minecraftVersionsCache;
		}

		ProjectInfoRequest? response = await httpClient.GetFromJsonAsync<ProjectInfoRequest>(prefixPath);

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		minecraftVersionsCache = response.Versions!;
		Array.Reverse(minecraftVersionsCache);

		return minecraftVersionsCache;
	}

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult($"https://api.purpurmc.org/v2/purpur/{minecraftVersion}/{build}/download");
	}

	internal record class BuildsInfo(
		string? Latest = null,
		string[]? All = null);

	internal record BuildInfoRequest(
		string? Project = null,
		string? Version = null,
		BuildsInfo? Builds = null);

	internal record ProjectInfoRequest(
		string? Project = null,
		string[]? Versions = null);
}