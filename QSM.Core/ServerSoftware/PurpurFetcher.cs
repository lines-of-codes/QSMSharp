using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

public class PurpurFetcher(IHttpClientFactory factory) : InfoFetcher
{
	public override string HttpClientName => "PurpurFetcher";
	public override string HttpBaseAddress => "https://api.purpurmc.org/v2/purpur/";

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (BuildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		using HttpClient client = factory.CreateClient(HttpClientName);
		BuildInfoRequest? response = await client.GetFromJsonAsync<BuildInfoRequest>(minecraftVersion);

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
		ProjectInfoRequest? response = await client.GetFromJsonAsync<ProjectInfoRequest>("");

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		MinecraftVersionsCache = response.Versions!;
		Array.Reverse(MinecraftVersionsCache);

		return MinecraftVersionsCache;
	}

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult($"https://api.purpurmc.org/v2/purpur/{minecraftVersion}/{build}/download");
	}

	internal sealed record BuildsInfo(
		string? Latest = null,
		string[]? All = null);

	internal sealed record BuildInfoRequest(
		string? Project = null,
		string? Version = null,
		BuildsInfo? Builds = null);

	internal sealed record ProjectInfoRequest(
		string? Project = null,
		string[]? Versions = null);
}