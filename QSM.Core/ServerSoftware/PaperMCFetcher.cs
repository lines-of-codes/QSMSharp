using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

/// <summary>
///     An information fetcher that can fetch PaperMC's project (Paper and Velocity)
/// </summary>
public class PaperMCFetcher : InfoFetcher
{
	private readonly HttpClient httpClient;

	private string project;

	public PaperMCFetcher(string project)
	{
		this.project = project;
		httpClient = new HttpClient { BaseAddress = new Uri($"https://api.papermc.io/v2/projects/{project}/") };
	}

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (buildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		AvailableBuildsRequest? response =
			await httpClient.GetFromJsonAsync<AvailableBuildsRequest>($"versions/{minecraftVersion}");

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		string[] builds = Array.ConvertAll(response.Builds!, num => num.ToString());
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

		ProjectInformation? response = await httpClient.GetFromJsonAsync<ProjectInformation>("");

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
		return Task.FromResult(
			$"{httpClient.BaseAddress!.ToString()}versions/{minecraftVersion}/builds/{build}/downloads/paper-{minecraftVersion}-{build}.jar");
	}

	internal record class AvailableBuildsRequest(
		string? project_id = null,
		string? project_name = null,
		string? version = null,
		int[]? builds = null)
	{
		public int[]? Builds = builds;
		public string? ProjectId = project_id;
		public string? ProjectName = project_name;
		public string? Version = version;
	}

	internal record class ProjectInformation(
		string? project_id = null,
		string? project_name = null,
		string[]? version_groups = null,
		string[]? versions = null)
	{
		public string? ProjectId = project_id;
		public string? ProjectName = project_name;
		public string[]? VersionGroups = version_groups;
		public string[]? Versions = versions;
	}
}