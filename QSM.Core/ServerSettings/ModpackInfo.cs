namespace QSM.Core.ServerSettings;

public class ModpackInfo
{
	public ModpackInfo() {}
	
	public ModpackInfo(string id, string name, string versionId, string versionName)
	{
		Id = id;
		Name = name;
		VersionId = versionId;
		VersionName = versionName;
	}
	
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string VersionId { get; set; } = string.Empty;
	public string VersionName { get; set; } = string.Empty;
}