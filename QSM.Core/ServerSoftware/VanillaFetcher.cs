using PasteMystNet;
using System.Diagnostics;
using System.Text.Json;

namespace QSM.Core.ServerSoftware;

public class VanillaFetcher : InfoFetcher
{
	private readonly Dictionary<string, string> _downloadUrls = new();
	private readonly PasteMystClient _pasteMystClient;

	public VanillaFetcher()
	{
		_pasteMystClient = new PasteMystClient();
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

		PasteMystPaste? paste = await _pasteMystClient.GetPasteAsync("1m9xuwik");

		if (paste == null)
		{
			throw new NetworkResourceUnavailableException();
		}

		Debug.WriteLine(paste.Pasties[0].Content);

		string[][]? response = JsonSerializer.Deserialize<string[][]>(paste.Pasties.First().Content);

		List<string> versions = [];

		for (ushort i = 1; i < response!.Length; i++)
		{
			string[] entry = response[i];
			versions.Add(entry[0]);
			_downloadUrls[entry[0]] = entry[1];
		}

		minecraftVersionsCache = versions.ToArray();

		return minecraftVersionsCache;
	}

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult(_downloadUrls[minecraftVersion]);
	}
}