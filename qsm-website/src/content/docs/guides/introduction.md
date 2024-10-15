---
title: Introduction
description: An introduction about what QSM is.
---

QSM (Quick Server Manager) is a server management software
for the game Minecraft. Currently, It is a Windows-only
program, but after the Windows version is stable,
QSM for Linux will definitely follow.

**QSM does not support hosting a Bedrock Edition Minecraft
server.** But you still can host a Java server with a plugin
which will make Bedrock edition players can join.

Even if QSM eases the process of self-hosting a Minecraft
server, it is still recommended that you understand some basics
of hosting a game server.

The Minecraft Wiki states the following ([source](https://minecraft.wiki/w/Tutorials/Setting_up_a_server#Warning)):

> In order to run your server and stay out of trouble, we highly suggest that you should at least know about the following:
> - Using the command-line and editing configuration files
> - Networking in general (IP, DHCP, ports, etc.)
> - Your system configuration
> - Your network configuration
> - Your router configuration (if you want other people to connect over the Internet)

(Planned) QSM features are of the following:
- [ ] Basic server controls & Configurations
- [ ] Java installation management
- [ ] Backups Management
- [ ] Mods/Plugins management

The documentation will include documentation for features
that are not implemented yet, is being implemented, and is
completed.

## License
QSM for Windows is licensed under [GNU GPL v3](https://github.com/lines-of-codes/QSMSharp/blob/master/LICENSE),
But the licenses for QSM (Windows) dependencies are the
following:

- Microsoft.WindowsAppSDK: [Microsoft Software License Terms for Windows App SDK](https://www.nuget.org/packages/Microsoft.WindowsAppSDK/1.5.240802000/license)
- Microsoft.Windows.SDK.BuildTools: [Microsoft Software License Terms for Microsoft Windows SDK for Windows 10](https://aka.ms/WinSDKLicenseURL)
- LiveChartsCore.SkiaSharpView.WinUI: [MIT License](https://github.com/beto-rodriguez/LiveCharts2/blob/master/LICENSE)
- ZstdSharp.Port: [MIT License](https://github.com/oleg-st/ZstdSharp/blob/master/LICENSE)

The QSM Core is licensed under the same GNU GPL v3,
And its dependencies has the following licenses:

- PasteMystNet: [MIT License](https://github.com/dentolos19/PasteMystNet/blob/main/LICENSE)
- System.Text.Json: [MIT License](https://licenses.nuget.org/MIT)

These lists only include direct/top-level dependencies.
