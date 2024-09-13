using QSM.Core.ServerSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Core.ModPluginSource
{
    public abstract class ModPluginProvider
    {
        protected ServerMetadata ServerMetadata;

        public ModPluginProvider(ServerMetadata serverMetadata)
        {
            ServerMetadata = serverMetadata;
        }

        public abstract Task<ModPluginInfo[]> Search(string query = "");
        /// <summary>
        /// Get available versions for 
        /// </summary>
        /// <param name="slug">The project's slug</param>
        /// <returns>Returns a dictionary in a Version:DownloadUrl format</returns>
        public abstract Task<ModPluginDownloadInfo[]> GetVersions(string slug);

        public abstract Task<ModPluginDownloadInfo> ResolveDependencies(ModPluginDownloadInfo mod);
    }
}
