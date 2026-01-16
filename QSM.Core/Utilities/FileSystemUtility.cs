namespace QSM.Core.Utilities;

public static class FileSystemUtility
{
	public static string GetTemporaryDirectory()
	{
		string tempDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

		if (Directory.Exists(tempDirectory))
		{
			return GetTemporaryDirectory();
		}

		Directory.CreateDirectory(tempDirectory);
		return tempDirectory;
	}
}
