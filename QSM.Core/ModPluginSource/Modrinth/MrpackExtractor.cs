using QSM.Core.Utilities;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using HashAlgorithm = QSM.Core.Utilities.HashAlgorithm;

namespace QSM.Core.ModPluginSource.Modrinth;

public class MrpackExtractor
{
	public static async IAsyncEnumerable<MrpackOperation> Install(string file, string temp, string dest)
	{
		(MrpackModrinthIndex Index, string ExtractLocation) extractResult = await ExtractAsync(file, temp);

		IAsyncEnumerable<MrpackOperation> downloader = DownloadMods(extractResult.Index, dest);

		await foreach (MrpackOperation op in downloader)
		{
			yield return op;
		}

		IEnumerable<MrpackOperation> copyOperation = CopyOverrides(extractResult.ExtractLocation, dest);

		foreach (MrpackOperation op in copyOperation)
		{
			yield return op;
		}

		Directory.Delete(extractResult.ExtractLocation, true);
	}

	/// <summary>
	/// </summary>
	/// <param name="file">A path to the .mrpack file</param>
	/// <param name="temp">A path to a folder to store temporary files</param>
	public static async Task<(MrpackModrinthIndex Index, string ExtractLocation)> ExtractAsync(string file, string temp)
	{
		string dest = Path.Combine(temp, Path.GetFileNameWithoutExtension(file));

		Directory.CreateDirectory(dest);

		await Task.Run(() => ZipFile.ExtractToDirectory(file, dest));

		string indexFile = Path.Combine(dest, "modrinth.index.json");

		if (!File.Exists(indexFile))
		{
			Directory.Delete(dest, true);
			throw new MrpackException("The .mrpack does not contain the modrinth.index.json file.");
		}

		MrpackModrinthIndex? index =
			JsonSerializer.Deserialize(File.ReadAllText(indexFile), MrpackContext.Default.MrpackModrinthIndex);

		if (index is null)
		{
			Directory.Delete(dest, true);
			throw new MrpackException("The .mrpack does not contain a valid modrinth.index.json file.");
		}

		return (index, dest);
	}

	public static Queue<FileDownloadRequest> GetModList(MrpackModrinthIndex index, string dest)
	{
		Queue<FileDownloadRequest> downloads = [];

		foreach (MrpackFile fileInfo in index.Files)
		{
			string fullPath = Path.GetFullPath(fileInfo.Path, dest);

			// Check if the full path escapes out of the Minecraft server instance directory
			if (!fullPath.StartsWith(dest))
			{
				continue;
			}

			Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

			downloads.Enqueue(new FileDownloadRequest
			{
				Destination = fullPath,
				DownloadLocations = fileInfo.Downloads.ToArray(),
				HashAlgorithm = HashAlgorithm.SHA512,
				Hash = fileInfo.Hashes["sha512"]
			});
		}

		return downloads;
	}

	public static async IAsyncEnumerable<MrpackOperation> DownloadMods(MrpackModrinthIndex index, string dest)
	{
		foreach (MrpackFile fileInfo in index.Files)
		{
			if (fileInfo.Env["server"] == "unsupported")
			{
				continue;
			}

			string fullPath = Path.GetFullPath(fileInfo.Path, dest);

			// Check if the full path escapes out of the Minecraft server instance directory
			if (!fullPath.StartsWith(dest))
			{
				continue;
			}

			Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

			foreach (string download in fileInfo.Downloads)
			{
				IAsyncEnumerable<MrpackOperation> enumerable = DownloadFile(download, fullPath);
				bool error = false;

				await foreach (MrpackOperation op in enumerable)
				{
					if (op.Error)
					{
						error = true;
						break;
					}

					yield return op;
				}

				if (!error)
				{
					using SHA512 sha512 = SHA512.Create();
					string hash = sha512.GetFileHashAsString(fullPath);

					if (hash != fileInfo.Hashes["sha512"])
					{
						error = true;
						yield return new MrpackOperation("Checksum doesn't match!");
					}
				}

				// Try the next download URL if the file can't be downloaded
				// or the checksum doesn't match.
				if (error)
				{
					continue;
				}

				// Breaks out of the loop if the file is successfully downloaded
				break;
			}
		}
	}

	public static IEnumerable<MrpackOperation> CopyOverrides(string extractLocation, string dest)
	{
		string overrides = Path.Combine(extractLocation, "overrides");
		string serverOverrides = Path.Combine(extractLocation, "server-overrides");
		DirectoryInfo destInfo = new(dest);

		if (Directory.Exists(overrides))
		{
			yield return new MrpackOperation("Copying overrides directory...");
			CopyDirectoryContents(new DirectoryInfo(overrides), destInfo);
		}

		if (Directory.Exists(serverOverrides))
		{
			yield return new MrpackOperation("Copying server overrides directory...");
			CopyDirectoryContents(new DirectoryInfo(serverOverrides), destInfo);
		}
	}

	public static async IAsyncEnumerable<MrpackOperation> DownloadFile(string url, string dest)
	{
		using HttpClient client = new();

		using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

		if (!response.IsSuccessStatusCode)
		{
			yield return new MrpackOperation("HTTP request status code does not indicate success.", true);
			yield break;
		}

		long totalBytes = response.Content.Headers.ContentLength ?? -1L;

		await using Stream contentStream = await response.Content.ReadAsStreamAsync();
		await using FileStream fileStream = File.Create(dest);

		string fileName = Path.GetFileName(dest);
		byte[] buffer = new byte[8192];
		long totalBytesRead = 0;
		int bytesRead;

		if (totalBytes != -1)
		{
			yield return new MrpackOperation($"Downloading {fileName}...", null);
		}

		while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
		{
			await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
			totalBytesRead += bytesRead;

			if (totalBytes != -1)
			{
				double percentage = (double)totalBytesRead / totalBytes * 100;
				yield return new MrpackOperation($"Downloading {fileName}...", percentage);
			}
		}
	}

	public static void CopyDirectoryContents(DirectoryInfo source, DirectoryInfo dest)
	{
		foreach (DirectoryInfo dir in source.GetDirectories())
		{
			CopyDirectoryContents(dir, dest.CreateSubdirectory(dir.Name));
		}

		foreach (FileInfo file in source.GetFiles())
		{
			file.CopyTo(Path.Combine(dest.FullName, file.Name), true);
		}
	}
}