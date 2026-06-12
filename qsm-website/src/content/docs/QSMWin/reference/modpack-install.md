---
title: Modpack Installation
description: How QSM installs modpack.
---

QSM supports installing modpacks from Modrinth and CurseForge.

Note that this implementation is imperfect, and there are
better ways to gracefully fail to do a thing.

When "abort" is mentioned in this document, It aborts the operation and will 
either show a dialog or tell what is wrong in the QSM log, so it is important 
that you should check the log files if suddenly nothing happens and your server
with your modpack is not created. This will keep being improved upon for a 
smoother user experience.

## Modrinth Modpack

After the user selects the desired modpack and clicks
"Confirm," the app does the following:

1. Check if the modpack file is in the `.mrpack` file extension. If it is not, abort.
2. Download the modpack file unless it's cached.
3. Check if the hash of the modpack file matches the hash sent from Modrinth. If not, abort.
4. Install the modpack
5. Clean up

The process of installing the modpack includes:

1. Extracting the `.mrpack` file
2. Check if it has a valid `modrinth.index.json`, If not, abort and clean up.
3. Parse the `modrinth.index.json`, If it fails, abort and clean up.
4. Download the mods specified in the index file to a location specified in the server instance. (located in `QSM/Servers/[modpack-name]`) If a mod fails to download or the mod is trying to be downloaded and placed outside of the server instance, the mod is skipped.
5. Copy the `overrides`, then `server-overrides` to the server location.
6. Download the server file (mod loader and version specified in the `modrinth.index.json` in the `dependencies` field)
