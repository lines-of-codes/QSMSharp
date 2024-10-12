using QSM.Core.ServerSoftware;
using System.Collections.Generic;

namespace QSM.Windows;

internal class ApplicationConfiguration
{
    public string MonospaceFont { get; set; } = "Consolas";

    public List<ServerMetadata> Servers { get; set; } = [];
    public List<string> JavaInstallations { get; set; } = [];
}
