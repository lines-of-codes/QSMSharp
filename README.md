# QSMSharp

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=lines-of-codes_QSMSharp&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=lines-of-codes_QSMSharp)

A Quick Server Manager for Minecraft, ironically written in C# and not Java.

This software plans to have basic server management and configuration,
mods/plugins management with modpack installation, and Java installation management.

This software stemmed from the fact that I love Prism Launcher's mod management
and wished for an open-source Minecraft server management with the same level of
capabilities.

## Projects

In the solution, There are three projects including:

- QSM.Core: Common classes shared between projects
- QSM.Web: Cross-platform web version of the management software
- QSM.Windows: Original Windows-only version, More complete.

## Forks

Similarly to Prism Launcher's policy, You are free to fork, redistribute, and 
provide custom builds as long as you follow the terms of the license (GNU GPLv3) 
and if you made code changes, please do the following as a basic courtesy:

- Make it clear that you're a fork and not affiliated with QSMSharp
- Remove QSMSharp's API keys

Currently, the only public API key used in QSMSharp is the CurseForge API key, 
which can be changed in the [QSM.Web/appsettings.jsonc](QSM.Web/appsettings.jsonc) 
or [QSM.Windows/appsettings.jsonc](QSM.Windows/appsettings.jsonc) file, depending on 
which project you are building upon. Note that removing the key will likely cause every 
method in the class to throw an exception due to being unauthorized.

If you are making use of the QSM.Core project, You will have to configure the HTTP 
client for CurseForge yourself. The QSM.Web/Windows project does the following:

```cs
builder.Services.AddHttpClient(CurseForgeProvider.HttpClientName, client =>
{
	client.BaseAddress = new Uri(CurseForgeProvider.BaseAddress);
	client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration.GetValue<string>("QSM:CurseForgeKey"));
});
```

You are free to change the source of the CurseForge key to anywhere, hardcoded in code, 
environment variables, or some other text file.

## License

The project is licensed under the GNU GPLv3, in plain text in the [LICENSE](LICENSE) file,
and available in RTF format for the ease of installer creation in [QSM.Windows/gpl-3.0.rtf](QSM.Windows/gpl-3.0.rtf)
(sourced from [gnu.org](https://www.gnu.org/licenses/gpl-3.0.html))

QSM.Windows/CodeDependencies.iss is from [InnoDependencyInstaller](https://github.com/DomGries/InnoDependencyInstaller)
and is licensed under the MIT license.
