using QSM.Core;

namespace QSM.Windows
{
    internal class WinServerInfo : ServerInfo<SymbolImage>
    {
        public WinServerInfo(string name, SymbolImage icon, string path) : base(name, icon, path)
        {
        }
    }
}
