using JetBrains.Annotations;
using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

public class FabricFetcher(IHttpClientFactory factory) : InfoFetcher
{
	public override string HttpClientName => "FabricFetcher";
	public override string HttpBaseAddress => "https://meta.fabricmc.net/";

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (BuildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		using HttpClient client = factory.CreateClient(HttpClientName);
		AvailableFabricVersion[]? response =
			await client.GetFromJsonAsync<AvailableFabricVersion[]>($"/v2/versions/loader/{minecraftVersion}");

		BuildInfoCache[minecraftVersion] = [.. response!.Select(e => e.Loader!.Version!)];

		return BuildInfoCache[minecraftVersion];
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (MinecraftVersionsCache.Length != 0)
		{
			return MinecraftVersionsCache;
		}

		using HttpClient client = factory.CreateClient(HttpClientName);
		SupportedMinecraftVersion[]? response =
			await client.GetFromJsonAsync<SupportedMinecraftVersion[]>("/v2/versions/game") 
				?? throw new NetworkResourceUnavailableException();
		List<string> versions = [];

		foreach (SupportedMinecraftVersion version in response)
		{
			if ((bool)version.Stable!)
			{
				versions.Add(version.Version!);
			}
		}

		MinecraftVersionsCache = [.. versions];

		return MinecraftVersionsCache;
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		using HttpClient client = factory.CreateClient(HttpClientName);
		FabricInstaller[]? response = await client.GetFromJsonAsync<FabricInstaller[]>("/v2/versions/installer");

		return response == null ? throw new NetworkResourceUnavailableException() : $"https://meta.fabricmc.net/v2/versions/loader/{minecraftVersion}/{build}/{response[0].Version}/server/jar";
	}

	[UsedImplicitly]
	private sealed record FabricInstaller(
		string? Url = null,
		string? Maven = null,
		string? Version = null,
		bool? Stable = null);

	[UsedImplicitly]
	private sealed record FabricLoader(
		string? Separator = null,
		int? Build = null,
		string? Maven = null,
		string? Version = null,
		bool? Stable = null);

	[UsedImplicitly]
	private sealed record AvailableFabricVersion(
		FabricLoader? Loader = null);

	[UsedImplicitly]
	private sealed record SupportedMinecraftVersion(
		string? Version = null,
		bool? Stable = null);
}