namespace QSM.Core
{
    public class ServerInfo<IconType>
    {
        public string Name;
        public IconType Icon;
        /// <summary>
        /// Path to server's directory
        /// </summary>
        public string Path;

        public ServerInfo(string name, IconType icon, string path)
        {
            Name = name;
            Icon = icon;
            Path = path;
        }
    }
}
