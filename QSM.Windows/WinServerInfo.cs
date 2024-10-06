using QSM.Core.ServerSoftware;

namespace QSM.Windows;

internal class WinServerInfo
{
    public SymbolImage Icon;
    public ServerMetadata Metadata;

    public WinServerInfo(SymbolImage icon, ServerMetadata metadata)
    {
        Icon = icon;
        Metadata = metadata;
    }
}
