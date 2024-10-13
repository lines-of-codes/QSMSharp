using System.IO;
using System.Text.RegularExpressions;

namespace QSM.Windows.Utilities;

internal partial class StringUtility
{
	static Regex invalidFileNameCheck = InvalidFileNameCheck();

	public static string TurnIntoValidFileName(string original)
	{
		string result = invalidFileNameCheck.Replace(original, "");

		if (result.Length == 0)
		{
			return Path.GetRandomFileName();
		}

		return result;
	}

	[GeneratedRegex("[\\\\/<>:|\\\\?\\\\*\\\"\\0]|^(PRN|AUX|NUL|CON)$|^(COM|LPT)[\\d¹²³]{1}$|[ .]$")]
	private static partial Regex InvalidFileNameCheck();
}
