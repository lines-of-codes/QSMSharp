using QSM.Core.ServerSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Core.ModPluginSource
{
    public class PaperMCHangarProvider : ModPluginProvider
    {
        internal record class PaginationInfo(
            int? Limit = null,
            int? Offset = null,
            int? Count = null);

        internal record class HangarNamespace(
            string? Owner = null,
            string? Slug = null);
        
        internal record class HangarStats(
            int? Views = null,
            int? Downloads = null,
            int? RecentViews = null,
            int? RecentDownloads = null,
            int? Stars = null,
            int? Watchers = null);

        internal record class HangarLicense(
            string? Name = null,
            string? Url = null,
            string? Type = null);

        internal record class HangarSettings(
            string[]? Tags = null,
            HangarLicense? License = null,
            string[]? Keywords = null,
            string? Sponsors = null);

        internal record class HangarProject(
            DateTime? CreatedAt = null,
            string? Name = null,
            HangarNamespace? Namespace = null,
            HangarStats? Stats = null,
            string? Category = null,
            DateTime? LastUpdated = null,
            string? Visibility = null,
            string? AvatarUrl = null,
            string? Description = null,
            HangarSettings? Settings = null);

        internal record class ProjectReleaseChannel(
            string? Name = null);

        internal record class HangarFileInfo(
            string? Name = null,
            int? SizeBytes = null,
            string? Sha256Hash = null);

        internal record class HangarDownloadEntry(
            HangarFileInfo? FileInfo = null,
            string? ExternalUrl = null,
            string? DownloadUrl = null);

        internal record class Dependency(
            string? Name = null,
            bool? Required = null,
            string? ExternalUrl = null,
            string? Platform = null);

        internal record class ProjectVersionEntry(
            DateTime? CreatedAt = null,
            string? Name = null,
            ProjectReleaseChannel? Channel = null,
            Dictionary<string, HangarDownloadEntry>? Downloads = null,
            Dictionary<string, Dependency[]>? PluginDependencies = null);

        internal record class SearchRequest(
            PaginationInfo? Pagination = null,
            HangarProject[]? Result = null);

        internal record class VersionRequest(
            PaginationInfo? Pagination = null,
            ProjectVersionEntry[]? Result = null);

        HttpClient HttpClient;

        public PaperMCHangarProvider(ServerMetadata serverMetadata) : base(serverMetadata)
        {
            HttpClient = new()
            {
                BaseAddress = new Uri("https://hangar.papermc.io/api/v1/")
            };
        }

        public override async Task<ModPluginDownloadInfo[]> GetVersions(string slug)
        {
            VersionRequest response = await HttpClient.GetFromJsonAsync<VersionRequest>(
                $"projects/{slug}/versions?platform={ServerMetadata.Software.ToString()}&platformVersion={ServerMetadata.MinecraftVersion}")
                ?? throw new NullReferenceException();

            List<ModPluginDownloadInfo> versions = [];

            foreach (ProjectVersionEntry version in response.Result!)
            {
                string platform = ServerMetadata.Software.ToString().ToUpper();
                HangarDownloadEntry downloadEntry = version.Downloads![platform];
                Dependency[] dependencies = version.PluginDependencies![platform];

                IEnumerable<ModPluginDownloadInfo.Dependency> genericInfo = dependencies.Select(dependency =>
                    new ModPluginDownloadInfo.Dependency()
                    {
                        Name = dependency.Name!,
                        DownloadUri = null,
                        ExternalPageUrl = dependency.ExternalUrl,
                        Required = (bool)dependency.Required!
                    }
                );

                versions.Add(new()
                {
                    DisplayName = $"{version.Name!} ({version.Channel!.Name})",
                    FileName = downloadEntry.FileInfo!.Name!,
                    DownloadUri = downloadEntry.DownloadUrl,
                    ExternalPageUrl = downloadEntry.ExternalUrl,
                    Dependencies = genericInfo.ToArray(),
                    Hash = downloadEntry.FileInfo.Sha256Hash
                });
            }

            return versions.ToArray();
        }

        public override async Task<ModPluginInfo[]> Search(string query = "")
        {
            SearchRequest response = await HttpClient.GetFromJsonAsync<SearchRequest>("projects") 
                ?? throw new NullReferenceException();

            List<ModPluginInfo> plugins = [];

            foreach (var project in response.Result!)
            {
                plugins.Add(new()
                {
                    Name = project.Name!,
                    IconUrl = project.AvatarUrl!,
                    License = project.Settings!.License!.Name!,
                    LicenseUrl = project.Settings.License.Url!,
                    Owner = project.Namespace!.Owner!,
                    Slug = project.Namespace.Slug!,
                    DownloadCount = (uint)project.Stats!.Downloads!,
                    Description = project.Description!
                });
            }

            return plugins.ToArray();
        }

        public override async Task<ModPluginDownloadInfo> ResolveDependencies(ModPluginDownloadInfo mod)
        {
            var resolvedDependencies = await Task.WhenAll(mod.Dependencies.Select(async dependency =>
            {
                Uri? downloadUri = null;

                if (dependency.ExternalPageUrl == null)
                {
                    downloadUri = new Uri((await GetVersions(dependency.Name!)).First().DownloadUri!);
                }

                return new ModPluginDownloadInfo.Dependency()
                {
                    Name = dependency.Name!,
                    DownloadUri = downloadUri,
                    ExternalPageUrl = dependency.ExternalPageUrl,
                    Required = dependency.Required!
                };
            }));

            mod.Dependencies = resolvedDependencies.ToArray();

            return mod;
        }
    }
}
