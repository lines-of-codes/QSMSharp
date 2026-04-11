using QSM.Core.Utilities;

namespace QSM.Core;

public struct FileDownloadRequest
{
	public string Destination { get; set; }
	public string[] DownloadLocations { get; set; }
	public HashAlgorithm HashAlgorithm { get; set; }
	public string Hash { get; set; }
}