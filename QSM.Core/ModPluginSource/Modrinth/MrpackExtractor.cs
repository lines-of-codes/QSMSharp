using QSM.Core.Utilities;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using HashAlgorithm = QSM.Core.Utilities.HashAlgorithm;

namespace QSM.Core.ModPluginSource.Modrinth
{
	public class MrpackExtractor
	{
		public static async IAsyncEnumerable<MrpackOperation> Install(string file, string temp, string dest)
		{
			var extractResult = await ExtractAsync(file, temp);
			
			var downloader = DownloadMods(extractResult.Index, dest);

			await foreach (var op in downloader)
			{
				yield return op;
			}

			var copyOperation = CopyOverrides(extractResult.ExtractLocation, dest);

			foreach (var op in copyOperation)
			{
				yield return op;
			}

			Directory.Delete(extractResult.ExtractLocation, true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file">A path to the .mrpack file</param>
		/// <param name="temp">A path to a folder to store temporary files</param>
		public static async Task<(MrpackModrinthIndex Index, string ExtractLocation)> ExtractAsync(string file, string temp)
		{
			var dest = Path.Combine(temp, Path.GetFileNameWithoutExtension(file));

			Directory.CreateDirectory(dest);
			
			await Task.Run(() => ZipFile.ExtractToDirectory(file, dest));

			var indexFile = Path.Combine(dest, "modrinth.index.json");

			if (!File.Exists(indexFile))
			{
				Directory.Delete(dest, true);
				throw new MrpackException("The .mrpack does not contain the modrinth.index.json file.");
			}

			var index = JsonSerializer.Deserialize(File.ReadAllText(indexFile), MrpackContext.Default.MrpackModrinthIndex);

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

			foreach (var fileInfo in index.Files)
			{
				string fullPath = Path.GetFullPath(fileInfo.Path, dest);

				// Check if the full path escapes out of the Minecraft server instance directory
				if (!fullPath.StartsWith(dest))
				{
					continue;
				}

				Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

				downloads.Enqueue(new()
				{
					Destination = fullPath,
					DownloadLocations = fileInfo.Downloads,
					HashAlgorithm = HashAlgorithm.SHA512,
					Hash = fileInfo.Hashes["sha512"]
				});
			}

			return downloads;
		}

		public static async IAsyncEnumerable<MrpackOperation> DownloadMods(MrpackModrinthIndex index, string dest)
		{
			foreach (var fileInfo in index.Files)
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

				foreach (var download in fileInfo.Downloads)
				{
					var enumerable = DownloadFile(download, fullPath);
					var error = false;

					await foreach (var op in enumerable)
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
						using var sha512 = SHA512.Create();
						var hash = sha512.GetFileHashAsString(fullPath);

						if (hash != fileInfo.Hashes["sha512"])
						{
							error = true;
							yield return new("Checksum doesn't match!");
						}
					}

					// Try the next download URL if the file can't be downloaded
					// or the checksum doesn't match.
					if (error)
						continue;

					// Breaks out of the loop if the file is successfully downloaded
					break;
				}
			}
		}

		public static IEnumerable<MrpackOperation> CopyOverrides(string extractLocation, string dest)
		{
			var overrides = Path.Combine(extractLocation, "overrides");
			var serverOverrides = Path.Combine(extractLocation, "server-overrides");
			var destInfo = new DirectoryInfo(dest);

			if (Directory.Exists(overrides))
			{
				yield return new("Copying overrides directory...");
				CopyDirectoryContents(new DirectoryInfo(overrides), destInfo);
			}

			if (Directory.Exists(serverOverrides))
			{
				yield return new("Copying server overrides directory...");
				CopyDirectoryContents(new DirectoryInfo(serverOverrides), destInfo);
			}
		}

		public static async IAsyncEnumerable<MrpackOperation> DownloadFile(string url, string dest)
		{
			using HttpClient client = new();

			using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			
			if (!response.IsSuccessStatusCode)
			{
				yield return new("HTTP request status code does not indicate success.", true);
				yield break;
			}

			long totalBytes = response.Content.Headers.ContentLength ?? -1L;

			using var contentStream = await response.Content.ReadAsStreamAsync();
			using var fileStream = File.Create(dest);

			var fileName = Path.GetFileName(dest);
			var buffer = new byte[8192];
			long totalBytesRead = 0;
			int bytesRead;
			double percentage = 0;

			if (totalBytes != -1)
			{
				yield return new($"Downloading {fileName}...", null);
			}

			while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
			{
				await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
				totalBytesRead += bytesRead;

				if (totalBytes != -1)
				{
					percentage = (double)totalBytesRead / totalBytes * 100;
					yield return new($"Downloading {fileName}...", percentage);
				}
			}
		}

		public static void CopyDirectoryContents(DirectoryInfo source, DirectoryInfo dest)
		{
			foreach (DirectoryInfo dir in source.GetDirectories())
				CopyDirectoryContents(dir, dest.CreateSubdirectory(dir.Name));

			foreach (FileInfo file in source.GetFiles())
				file.CopyTo(Path.Combine(dest.FullName, file.Name), true);
		}
	}
}
