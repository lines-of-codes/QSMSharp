namespace QSM.Core.ServerSoftware
{
    public abstract class InfoFetcher
    {
        protected string[] minecraftVersionsCache = [];
        protected Dictionary<string, string[]> buildInfoCache = [];

        public abstract Task<string[]> FetchAvailableMinecraftVersions();
        /// <summary>
        /// A function to fetch available software builds for a specific version of Minecraft.
        /// </summary>
        /// <returns>
        /// A dictionary where the keys are the build number and the values are the download link to the build
        /// </returns>
        public abstract Task<string[]> FetchAvailableBuilds(string minecraftVersion);

        public abstract Task<string> GetDownloadUrl(string minecraftVersion, string build);
    }
}
