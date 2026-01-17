---
title: Introduction
description: Introduction to the QSM Web software.
---

QSM.Web is a management console designed for self-hosted servers. Currently, QSM.Web does not have any published version, so you'll have to build the application manually.

## Build Steps

_This guide is very rough and is mainly for the Linux platform, contributions will be appreciated_

1. Download & Install the .NET SDK (https://dotnet.microsoft.com/en-us/download)
2. Use Git to clone the repository, or download a ZIP archive of the code from https://github.com/lines-of-codes/QSMSharp
3. `cd` into the `QSM.Web` directory within the cloned/extracted project
4. Run `dotnet publish -c Release`
5. The build files should be in the `bin/Release/net10.0/publish` folder. Copy the contents of that folder to somewhere like `/opt/qsm-web/` (for Linux). Make sure the permissions are correct and that it can write into the `Data/app.db` file (Modify the `appsettings.json` file to change the location of the database file)
6. Refer to [Application Data](/QSMSharp/qsmweb/reference/application-data) and create the folder which QSM will store most of its data to. (For example, on Linux, Create the `/var/lib/qsm-web` folder and change the ownership to the user that will be running QSM Web)
7. Run the `QSM.Web` executable! You can use `tmux` or write a systemd unit file to help with this.

By default, the web console will be started on port 5222 and will accept connections from anywhere. But do remember that if you want to access the console from other devices, you'll need to configure your firewall if you have one.

## Setting Up

Upon first entering http://localhost:5222, You will be prompted to create an account (This page will only appear once when there are no account registered on the server).

Next, Install Java. You'll have to do it manually, and after that, register your Java installation by going into QSM Settings &rarr; Java, then enter the folder which your Java install is located. (Said folder will contain directories like `bin`, `include`, and `lib`)

For convenience, You can make the newly added Java install the default by clicking the "Make Default" button.

## Technical Details

It relies on ASP.NET, Blazor, Entity Framework, and SQLite.
