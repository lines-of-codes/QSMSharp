using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware
{
    public class PurpurFetcher : InfoFetcher
    {
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

        HttpClient httpClient;

        public PurpurFetcher()
        {
            httpClient = new()
            {
                BaseAddress = new Uri("https://api.purpurmc.org/v2/purpur/")
            };
        }

        public override async Task<string[]> FetchAvailableBuilds(string minecraftVersion)
        {
            if (buildInfoCache.ContainsKey(minecraftVersion)) return buildInfoCache[minecraftVersion];

            BuildInfoRequest? response = await httpClient.GetFromJsonAsync<BuildInfoRequest>(minecraftVersion);

            if (response == null)
                throw new NullReferenceException();

            string[] builds = response.Builds!.All!;

            Array.Sort(builds, (a, b) => int.Parse(a) - int.Parse(b));
            Array.Reverse(builds);

            buildInfoCache[minecraftVersion] = builds;

            return builds;
        }

        public override async Task<string[]> FetchAvailableMinecraftVersions()
        {
            if (minecraftVersionsCache.Length != 0) return minecraftVersionsCache;

            ProjectInfoRequest? response = await httpClient.GetFromJsonAsync<ProjectInfoRequest>("");
            
            if (response == null)
                throw new NullReferenceException();

            minecraftVersionsCache = response.Versions!;
            Array.Reverse(minecraftVersionsCache);

            return minecraftVersionsCache;
        }
    }
}
