using QSM.Core.ServerSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Core.ModPluginSource
{
    public class ModrinthProvider : ModPluginProvider
    {
        internal record class VersionDependency(
            string? version_id = null,
            string? project_id = null,
            string? file_name = null,
            string? dependency_type = null);

        internal record class VersionFileHashes(
            string? sha512 = null,
            string? sha1 = null);

        internal record class VersionFile(
            VersionFileHashes? hashes = null,
            string? url = null,
            string? filename = null,
            bool? primary = null,
            int? size = null,
            string? file_type = null);

        internal record class VersionInfo(
            string? name = null,
            string? version_number = null,
            string? changelog = null,
            VersionDependency[]? dependencies = null,
            string? version_type = null,
            bool? featured = null,
            VersionFile[]? files = null);

        internal record class ProjectResult(
            string? slug = null,
            string? title = null,
            string? description = null,
            string[]? categories = null,
            string? client_side = null,
            string? server_side = null,
            string? project_type = null,
            int? downloads = null,
            string? icon_url = null,
            string? project_id = null,
            string? author = null,
            string[]? display_categories = null,
            string[]? versions = null,
            int? follows = null,
            DateTime? date_created = null,
            DateTime? date_modified = null,
            string? latest_version = null,
            string? license = null,
            string[]? gallery = null,
            string? featured_gallery = null);

        internal record class SearchRequest(
            ProjectResult[]? hits = null,
            int? offset = null,
            int? limit = null,
            int? total_hits = null)
        {
            public ProjectResult[]? Hits = hits;
            public int? Offset = offset;
            public int? Limit = limit;
            public int? TotalHits = total_hits;
        }

        HttpClient HttpClient;
        private static readonly string[] ignoredDependencyType = ["embedded", "incompatible"];

        public ModrinthProvider(ServerMetadata serverMetadata) : base(serverMetadata)
        {
            HttpClient = new()
            {
                BaseAddress = new Uri("https://api.modrinth.com/v2/")
            };

            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"lines-of-codes/WinQSM/{Assembly.GetEntryAssembly()!.GetName().Version} (linesofcodes@dailitation.xyz)");
        }

        public override async Task<ModPluginDownloadInfo[]> GetVersions(string slug)
        {
            string queryString = $"project/{slug}/version?loaders=";

            queryString += WebUtility.UrlEncode($"[\"{ServerMetadata.Software.ToString().ToLower()}\"]");
            queryString += "&game_versions=";
            queryString += WebUtility.UrlEncode($"[\"{ServerMetadata.MinecraftVersion}\"]");

            VersionInfo[] response = await HttpClient.GetFromJsonAsync<VersionInfo[]>(queryString)
                                            ?? throw new NullReferenceException();

            List<ModPluginDownloadInfo> versions = [];

            foreach (VersionInfo info in response)
            {
                var dependencies = info.dependencies!.Select(dependency => new ModPluginDownloadInfo.Dependency()
                {
                    Name = dependency.version_id ?? dependency.file_name!,
                    DownloadUri = null,
                    ExternalPageUrl = dependency.dependency_type,
                    Required = dependency.dependency_type == "required"
                });

                VersionFile primaryFile = info.files!.First(file => (bool)file.primary!);

                versions.Add(new()
                {
                    DisplayName = $"{info.name} ({info.version_type})",
                    FileName = primaryFile.filename!,
                    Dependencies = dependencies.ToArray(),
                    DownloadUri = primaryFile.url,
                    ExternalPageUrl = null,
                    Hash = primaryFile.hashes!.sha512
                });
            }

            return versions.ToArray();
        }

        public override async Task<ModPluginInfo[]> Search(string query = "")
        {
            string queryString = "search";

            List<List<string>> facets = new();

            List<string> projectType = new();
            if (ServerMetadata.IsModSupported)
            {
                projectType.Add("project_type:mod");
            }

            if (ServerMetadata.IsPluginSupported)
            {
                projectType.Add("project_type:plugin");
            }
            facets.Add(projectType);

            facets.Add([$"versions:{ServerMetadata.MinecraftVersion}"]);

            facets.Add([$"categories:{ServerMetadata.Software.ToString().ToLower()}"]);

            facets.Add(["server_side!=unsupported"]);

            StringBuilder sb = new();

            sb.Append('[');
            foreach (var facet in facets)
            {
                sb.Append('[');
                foreach (var entry in facet)
                {
                    sb.Append($"\"{entry}\"");
                }
                sb.Append("],");
            }
            sb.Remove(sb.Length - 1, 1); // Remove the last trailing comma
            sb.Append(']');

            queryString += "?facets=";
            queryString += WebUtility.UrlEncode(sb.ToString());

            if (!string.IsNullOrWhiteSpace(query))
            {
                queryString += "&query=";
                queryString += WebUtility.UrlEncode(query);
            }

            SearchRequest response = await HttpClient.GetFromJsonAsync<SearchRequest>(queryString)
                                            ?? throw new NullReferenceException();

            List<ModPluginInfo> modPlugins = [];

            foreach (var project in response.Hits!)
            {
                modPlugins.Add(new()
                {
                    IconUrl = project.icon_url!,
                    License = project.license!,
                    Name = project.title!,
                    Owner = project.author!,
                    Slug = project.slug!,
                    DownloadCount = (uint)project.downloads!,
                    LicenseUrl = project.license!.StartsWith("LicenseRef") ? string.Empty : $"https://spdx.org/licenses/{project.license}",
                    Description = project.description!
                });
            }

            return modPlugins.ToArray();
        }

        public override async Task<ModPluginDownloadInfo> ResolveDependencies(ModPluginDownloadInfo mod)
        {
            var resolvedDependencies = (await Task.WhenAll(mod.Dependencies.Select(async dependency =>
            {
                Uri? downloadUri = null;

                if (!ignoredDependencyType.Contains(dependency.ExternalPageUrl))
                {
                    VersionInfo response = await HttpClient.GetFromJsonAsync<VersionInfo>($"version/{dependency.Name}")
                                            ?? throw new NullReferenceException();

                    downloadUri = new Uri(response.files!.First(file => (bool)file.primary!).url!);
                }

                return new ModPluginDownloadInfo.Dependency()
                {
                    Name = dependency.Name,
                    DownloadUri = downloadUri,
                    ExternalPageUrl = null,
                    Required = dependency.Required
                };
            })));

            mod.Dependencies = resolvedDependencies;

            return mod;
        }
    }
}
