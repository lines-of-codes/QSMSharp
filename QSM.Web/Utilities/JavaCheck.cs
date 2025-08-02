using QSM.Windows.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QSM.Web.Utilities;

public static class JavaCheck
{
	private static readonly string s_pathToJavaCheck = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "JavaCheck.jar");
	private static readonly string[] s_newLineSeparator = ["\r\n", "\n"];

	public static bool CheckJavaInstallation(string javaHome, out JavaInstallation install)
	{
		ProcessStartInfo startInfo = new()
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = Path.Combine(javaHome, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bin/java.exe" : "bin/java"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"-jar \"{s_pathToJavaCheck}\"",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			EnvironmentVariables = { ["JAVA_HOME"] = javaHome }
		};

		install = new JavaInstallation
		{
			Path = javaHome
		};

		try
		{
			using Process process = new();
			process.StartInfo = startInfo;

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
	
	public static async Task<JavaInstallation> CheckJavaInstallationAsync(string javaHome)
	{
		ProcessStartInfo startInfo = new()
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = Path.Combine(javaHome, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bin/java.exe" : "bin/java"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"-jar \"{s_pathToJavaCheck}\"",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			EnvironmentVariables = { ["JAVA_HOME"] = javaHome }
		};

		JavaInstallation install = new JavaInstallation
		{
			Path = javaHome
		};

		try
		{
			using Process process = new();
			process.StartInfo = startInfo;

			process.Start();
			string output = await process.StandardOutput.ReadToEndAsync();
			string error = await process.StandardOutput.ReadToEndAsync();
			await process.WaitForExitAsync();


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
				}
			}
		}
		catch (Win32Exception e)
		{
			Debug.WriteLine(e.Message);
		}

		return install;
	}
}
