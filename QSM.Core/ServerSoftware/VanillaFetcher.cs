using PasteMystNet;
using System.Diagnostics;
using System.Text.Json;

namespace QSM.Core.ServerSoftware;

public class VanillaFetcher : InfoFetcher
{
	private readonly Dictionary<string, string> DownloadUrls = new();
	private readonly PasteMystClient pasteMystClient;
	private HttpClient httpClient;

	public VanillaFetcher()
	{
		httpClient = new HttpClient();
		pasteMystClient = new PasteMystClient();
	}

	public override Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		return Task.FromResult<string[]>([]);
	}

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (minecraftVersionsCache.Length != 0)
		{
			return minecraftVersionsCache;
		}

		PasteMystPaste? paste = await pasteMystClient.GetPasteAsync("1m9xuwik");

		if (paste == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		Debug.WriteLine(paste.Pasties[0].Content);

		string[][]? response = JsonSerializer.Deserialize<string[][]>(paste.Pasties.First().Content);

		List<string> versions = [];

		for (ushort i = 1; i < response!.Length; i++)
		{
			string[] entry = response![i];
			versions.Add(entry[0]);
			DownloadUrls[entry[0]] = entry[1];
		}

		minecraftVersionsCache = versions.ToArray();

		return minecraftVersionsCache;
	}

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult(DownloadUrls[minecraftVersion]);
	}
}