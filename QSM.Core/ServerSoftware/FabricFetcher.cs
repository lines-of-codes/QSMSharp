using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware;

public class FabricFetcher : InfoFetcher
{
	private readonly HttpClient httpClient;

	public FabricFetcher()
	{
		httpClient = new HttpClient { BaseAddress = new Uri("https://meta.fabricmc.net/v2/") };
	}

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (buildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return buildInfo;
		}

		AvailableFabricVersion[]? response =
			await httpClient.GetFromJsonAsync<AvailableFabricVersion[]>($"versions/loader/{minecraftVersion}");

		List<string> loaders = [];

		foreach (AvailableFabricVersion entry in response!)
		{
			loaders.Add(entry!.Loader!.Version!);
		}

		buildInfoCache[minecraftVersion] = loaders.ToArray();

		return buildInfoCache[minecraftVersion];
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (minecraftVersionsCache.Length != 0)
		{
			return minecraftVersionsCache;
		}

		SupportedMinecraftVersion[]? response =
			await httpClient.GetFromJsonAsync<SupportedMinecraftVersion[]>("versions/game");

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		List<string> versions = [];

		foreach (SupportedMinecraftVersion version in response)
		{
			if ((bool)version.Stable!)
			{
				versions.Add(version.Version!);
			}
		}

		minecraftVersionsCache = versions.ToArray();

		return minecraftVersionsCache;
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		FabricInstaller[]? response = await httpClient.GetFromJsonAsync<FabricInstaller[]>("versions/installer");

		if (response == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		return
			$"https://meta.fabricmc.net/v2/versions/loader/{minecraftVersion}/{build}/{response[0].Version}/server/jar";
	}

	internal record class FabricInstaller(
		string? Url = null,
		string? Maven = null,
		string? Version = null,
		bool? Stable = null);

	internal record class FabricLoader(
		string? Separator = null,
		int? Build = null,
		string? Maven = null,
		string? Version = null,
		bool? Stable = null);

	internal record class AvailableFabricVersion(
		FabricLoader? Loader = null);

	internal record class SupportedMinecraftVersion(
		string? Version = null,
		bool? Stable = null);
}