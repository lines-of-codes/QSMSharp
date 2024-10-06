using QSM.Windows.Pages.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Windows.Utilities
{
    public static class JavaCheck
    {
		private static string PathToJavaCheck = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "JavaCheck.jar");
		private static readonly string[] s_newLineSeparator = ["\r\n", "\n"];

		public static bool CheckJavaInstallation(string javaHome, out JavaInstallation install)
		{
			ProcessStartInfo startInfo = new()
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				FileName = Path.Combine(javaHome, "bin/javaw.exe"),
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"-jar {PathToJavaCheck}",
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			startInfo.EnvironmentVariables["JAVA_HOME"] = javaHome;

			install = new()
			{
				Path = javaHome
			};

			try
			{
				using (Process process = new Process { StartInfo = startInfo })
				{
					process.Start();
					string output = process.StandardOutput.ReadToEnd();
					string error = process.StandardOutput.ReadToEnd();
					process.WaitForExit();


					var properties = output.Split(s_newLineSeparator, StringSplitOptions.RemoveEmptyEntries);

					if (!string.IsNullOrWhiteSpace(error))
						Debug.WriteLine($"err: {error}");

					foreach (var rawProperty in properties)
					{
						string[] splitProperty = rawProperty.Split('=');

						if (splitProperty.Length < 2)
							break;

						KeyValuePair<string, string> parsedProperty = new(splitProperty[0], splitProperty[1]);

						switch (parsedProperty.Key)
						{
							case "java.version":
								install.Version = parsedProperty.Value;
								break;
							case "java.vendor":
								install.Vendor = parsedProperty.Value;
								break;
							default:
								break;
						}
					}
				}
			}
			catch (Win32Exception e)
			{
				Debug.WriteLine(e.Message);
				return false;
			}

			return true;
		}
	}
}
