using QSM.Core.ServerSoftware;
using System;

namespace QSM.Windows;

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
