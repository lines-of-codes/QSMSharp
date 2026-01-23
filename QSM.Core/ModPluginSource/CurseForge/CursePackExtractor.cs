using QSM.Core.ModPluginSource.Modrinth;
using System.IO.Compression;
using System.Text.Json;

namespace QSM.Core.ModPluginSource.CurseForge;

public class CursePackExtractor
{
	public string ExtractLocation { get; private set; } = string.Empty;
	private CursePackManifest? Manifest { get; set; }
	
	public async Task<CursePackManifest> ExtractAsync(string file, string temp)
	{
		ExtractLocation = Path.Join(temp, Path.GetFileNameWithoutExtension(file));

		Directory.CreateDirectory(ExtractLocation);

		await ZipFile.ExtractToDirectoryAsync(file, ExtractLocation);

		string manifestFile = Path.Join(ExtractLocation, "manifest.json");
		
		if (!File.Exists(manifestFile))
		{
			Directory.Delete(ExtractLocation, true);
			throw new CursePackException("The modpack does not contain the manifest.json file.");
		}

		await using FileStream fs = File.OpenRead(manifestFile);
		Manifest = await JsonSerializer.DeserializeAsync(fs, CursePackContext.Default.CursePackManifest);
		
		return Manifest ?? throw new CursePackException("The modpack does not contain a valid manifest.json file");
	}

	public void CopyOverrides(string dest)
	{
		string overrides = Path.Join(ExtractLocation, Manifest?.Overrides);
		DirectoryInfo destInfo = new(dest);

		if (!Directory.Exists(overrides))
			return;
	
		MrpackExtractor.CopyDirectoryContents(new DirectoryInfo(overrides), destInfo);
	}
}

public class CursePackException(string reason) : Exception(reason);