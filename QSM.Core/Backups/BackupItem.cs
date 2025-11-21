using Visus.Cuid;

namespace QSM.Core.Backups;

public class BackupItem
{
	public const byte DefaultCuidLength = 8;

	public BackupItem(Cuid2 id, string name, Uri uri, bool isSavedOnline = false)
	{
		Id = id.ToString();
		Name = name;
		SavedDate = DateTime.UtcNow;
		Uri = uri;
		IsSavedOnline = isSavedOnline;
	}

	public BackupItem(string name, Uri uri, bool isSavedOnline = false)
	{
		Id = new Cuid2(DefaultCuidLength).ToString();
		Name = name;
		SavedDate = DateTime.UtcNow;
		Uri = uri;
		IsSavedOnline = isSavedOnline;
	}

	public BackupItem()
	{
		Id = new Cuid2(DefaultCuidLength).ToString();
		Name = string.Empty;
		SavedDate = DateTime.UtcNow;
		Uri = new Uri("about:blank");
		IsSavedOnline = false;
	}

	public string Id { get; set; }
	public string Name { get; set; }
	public DateTime SavedDate { get; set; }

	/// <summary>
	///     The Uri to the backup
	/// </summary>
	public Uri Uri { get; set; }

	public bool IsSavedOnline { get; set; }
}