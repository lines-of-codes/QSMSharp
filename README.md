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
which can be changed in the [CurseForgeProvider.cs](QSM.Core/ModPluginSource/CurseForgeProvider.cs) 
file. Note that removing the key will likely cause every method in the class to
throw an exception due to being unauthorized.

## License

The project is licensed under the GNU GPLv3.

QSM.Windows/CodeDependencies.iss is from [InnoDependencyInstaller](https://github.com/DomGries/InnoDependencyInstaller)
and is licensed under CPOL 1.02.
