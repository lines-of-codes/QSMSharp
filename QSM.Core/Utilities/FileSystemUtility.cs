namespace QSM.Core.Utilities;

public static class FileSystemUtility
{
	public static string GetTemporaryDirectory()
	{
		while (true)
		{
			string tempDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

			if (Directory.Exists(tempDirectory))
			{
				continue;
			}

			Directory.CreateDirectory(tempDirectory);
			return tempDirectory;
		}
	}
}
