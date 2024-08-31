using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Core.ServerSoftware
{
    public class FabricFetcher : InfoFetcher
    {
        internal record class SupportedVersion(
            string? Version = null, 
            bool? Stable = null);

        HttpClient httpClient;

        public FabricFetcher()
        {
            httpClient = new()
            {
                BaseAddress = new Uri("https://meta.fabricmc.net/v2/")
            };
        }

        public override Task<string[]> FetchAvailableBuilds(string minecraftVersion)
        {
            throw new NotImplementedException();
        }

        public override async Task<string[]> FetchAvailableMinecraftVersions()
        {
            if (minecraftVersionsCache.Length != 0) return minecraftVersionsCache;

            SupportedVersion[]? response = await httpClient.GetFromJsonAsync<SupportedVersion[]>("versions/game");

            if (response == null)
                throw new NullReferenceException();

            List<string> versions = [];

            foreach (var version in response)
            {
                if ((bool)version.Stable!)
                    versions.Add(version.Version!);
            }

            minecraftVersionsCache = versions.ToArray();

            return minecraftVersionsCache;
        }
    }
}
