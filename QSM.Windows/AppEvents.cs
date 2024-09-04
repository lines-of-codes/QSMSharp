using QSM.Core.ServerSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Windows
{
    internal class AppEvents
    {
        public static event Action<ServerMetadata> NewServerAdded;

        public static void AddNewServer(ServerMetadata metadata)
        {
            ApplicationData.Configuration.Servers.Add(metadata);
            NewServerAdded?.Invoke(metadata);
            ApplicationData.SaveConfiguration();
        }
    }
}
