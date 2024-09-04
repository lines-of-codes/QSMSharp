using PasteMystNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QSM.Core.ServerSoftware
{
    public class VanillaFetcher : InfoFetcher
    {
        HttpClient httpClient;
        PasteMystClient pasteMystClient;
        Dictionary<string, string> DownloadUrls = new();

        public VanillaFetcher()
        {
            httpClient = new();
            pasteMystClient = new();
        }

        public override Task<string[]> FetchAvailableBuilds(string minecraftVersion)
        {
            return Task.FromResult<string[]>([]);
        }

        public override async Task<string[]> FetchAvailableMinecraftVersions()
        {
            if (minecraftVersionsCache.Length != 0) return minecraftVersionsCache;

            var paste = await pasteMystClient.GetPasteAsync("1m9xuwik");

            if (paste == null)
                throw new NullReferenceException();

            Debug.WriteLine(paste.Pasties.First().Content);

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

        public override Task<string> GetDownloadUrl(string minecraftVersion, string build)
        {
            return Task.FromResult(DownloadUrls[minecraftVersion]);
        }
    }
}
