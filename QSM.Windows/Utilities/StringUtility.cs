using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QSM.Windows.Utilities;

internal static partial class StringUtility
{
	static Regex s_invalidFileNameCheck = InvalidFileNameCheck();

	public static string TurnIntoValidFileName(string original)
	{
		string result = s_invalidFileNameCheck.Replace(original, string.Empty);

		if (result.Length == 0)
		{
			return Path.GetRandomFileName();
		}

		return result;
	}

	public static string KebabCaseToText(string text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		return string.Join(
			" ",
			text.Split('-')
				.Select(word => char.ToUpper(word[0]) + word[1..].ToLowerInvariant()));
	}

	public static string ToKebabCase(string text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		// First replace any underscores with spaces
		text = text.Replace('_', ' ');

		// Insert spaces before capital letters (handles camelCase and PascalCase)
		//text = Regex.Replace(text, new AddSpacesRegex(), "$1 $2", RegexOptions.None, TimeSpan.FromMilliseconds(100));

		// Convert to lowercase and replace spaces with hyphens
		return string.Join("-",
			text.Split([' '], StringSplitOptions.RemoveEmptyEntries)
				.Select(word => word.Trim().ToLower()));
	}

	[GeneratedRegex("[\\\\/<>:|\\\\?\\\\*\\\"\\0]|^(PRN|AUX|NUL|CON)$|^(COM|LPT)[\\d¹²³]{1}$|[ .]$")]
	private static partial Regex InvalidFileNameCheck();

	[GeneratedRegex("([a-z])([A-Z])")]
	private static partial Regex AddSpacesRegex();
}
