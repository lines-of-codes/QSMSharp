// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using QSM.Core.Utilities;

namespace QSM.Core;

public struct FileDownloadRequest
{
	public string Destination;
	public string[] DownloadLocations;
	public HashAlgorithm HashAlgorithm;
	public string Hash;
}
