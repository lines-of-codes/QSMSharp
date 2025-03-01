using System.IO;

namespace QSM.Windows.Utilities;

public static class FileSystemUtility
{
	public static string GetTemporaryDirectory()
	{
		string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		if (Directory.Exists(tempDirectory))
		{
			return GetTemporaryDirectory();
		}

		Directory.CreateDirectory(tempDirectory);
		return tempDirectory;
	}
}
