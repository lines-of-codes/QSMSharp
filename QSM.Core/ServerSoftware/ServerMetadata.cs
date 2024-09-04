namespace QSM.Core.ServerSoftware
{
    public class ServerMetadata
    {
        public string Name { get; set; } = "";
        public ServerSoftwares Software { get; init; }
        public string MinecraftVersion { get; init; } = "";
        public string ServerVersion { get; init; } = "";
        public string ServerPath { get; set; } = "";

        public ServerMetadata()
        {

        }

        public ServerMetadata(string name, ServerSoftwares software, string minecraftVersion, string serverVersion, string serverPath)
        {
            Name = name;
            Software = software;
            MinecraftVersion = minecraftVersion;
            ServerVersion = serverVersion;
            ServerPath = serverPath;
        }
    }
}
