using System.Net.Http.Json;

namespace QSM.Core.ServerSoftware
{
    /// <summary>
    /// An information fetcher that can fetch PaperMC's project (Paper and Velocity)
    /// </summary>
    public class PaperMCFetcher : InfoFetcher
    {
        internal record class AvailableBuildsRequest(
            string? project_id = null,
            string? project_name = null,
            string? version = null,
            int[]? builds = null)
        {
            public string? ProjectId = project_id;
            public string? ProjectName = project_name;
            public string? Version = version;
            public int[]? Builds = builds;
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

        HttpClient httpClient;

        public PaperMCFetcher(string project)
        {
            httpClient = new()
            {
                BaseAddress = new Uri($"https://api.papermc.io/v2/projects/{project}/")
            };
        }

        public override async Task<string[]> FetchAvailableBuilds(string minecraftVersion)
        {
            if (buildInfoCache.ContainsKey(minecraftVersion)) return buildInfoCache[minecraftVersion];

            AvailableBuildsRequest? response = await httpClient.GetFromJsonAsync<AvailableBuildsRequest>($"versions/{minecraftVersion}");

            if (response == null)
                throw new NullReferenceException();

            string[] builds = Array.ConvertAll(response.Builds!, num => num.ToString());
            Array.Reverse(builds);

            buildInfoCache[minecraftVersion] = builds;

            return builds;
        }

        public override async Task<string[]> FetchAvailableMinecraftVersions()
        {
            if (minecraftVersionsCache.Length != 0) return minecraftVersionsCache;

            ProjectInformation? response = await httpClient.GetFromJsonAsync<ProjectInformation>("");

            if (response == null)
                throw new NullReferenceException();

            minecraftVersionsCache = response.Versions!;

            Array.Reverse(minecraftVersionsCache);

            return minecraftVersionsCache;
        }
    }
}
