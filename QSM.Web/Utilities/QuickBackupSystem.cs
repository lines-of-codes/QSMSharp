using QSM.Web.Data;
using System.Runtime.InteropServices;

namespace QSM.Web.Utilities;

public class QuickBackupSystem
{
	public static string InstallPath()
	{
		string installPath = Path.Join(ApplicationConfig.GetDefaultAppDataFolder(), "qbs/qbsgo");

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			installPath += ".exe";
		}
		
		return installPath;
	}
	
	
}