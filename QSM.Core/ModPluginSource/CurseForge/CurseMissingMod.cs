namespace QSM.Core.ModPluginSource.CurseForge;

public struct CurseMissingMod(string slug, string name, uint fileId)
{
	public string Slug { get; set; } = slug;
	public string Name { get; set; } = name;
	public uint FileId { get; set; } = fileId;
	public readonly string Url => $"https://www.curseforge.com/minecraft/mc-mods/{Slug}/download/{FileId}";
}
