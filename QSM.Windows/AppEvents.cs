using QSM.Core.ServerSoftware;
using System;

namespace QSM.Windows;

internal class AppEvents
{
    public static event Action<ServerMetadata> NewServerAdded;
    public static event Action<ServerMetadata> ServerRemoved;

    public static void AddNewServer(ServerMetadata metadata)
    {
        ApplicationData.Configuration.Servers.Add(metadata);

        NewServerAdded?.Invoke(metadata);
        
        ApplicationData.SaveConfiguration();
    }

    public static void RemoveServer(ServerMetadata metadata)
    {
        ApplicationData.Configuration.Servers.Remove(metadata);
        
        ServerRemoved?.Invoke(metadata);

		ApplicationData.ServerSettings.Remove(metadata.Guid);

		ApplicationData.SaveConfiguration();
    }
}
