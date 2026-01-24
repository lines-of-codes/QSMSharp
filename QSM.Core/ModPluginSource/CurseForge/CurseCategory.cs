namespace QSM.Core.ModPluginSource.CurseForge;
public record CurseCategory(
		uint Id,
		uint GameId,
		string Name,
		string Slug,
		string Url,
		string IconUrl,
		string DateModified,
		bool? IsClass,
		uint? ClassId,
		uint? ParentCategoryId,
		int? DisplayIndex);
