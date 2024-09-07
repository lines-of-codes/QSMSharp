---
title: Choosing a server software
description: How to choose a software for your Minecraft server.
---

There are two types of server softwares in QSM. **Proxies**
and **Game servers**.

## Game servers
**Game servers** are the core server software that hosts the
game world, manage player data, etc.

Game servers can support datapacks, mods, and/or plugins.

**Mods** are _usually_ modifications that modify the core game
and require both the client and the server to have the mods
installed. But some mods are an exception to this, as some mods
are client-side/server-side only. Some examples include:

- [The Create Mod](https://modrinth.com/mod/create) - Adding new blocks and mechanics, Requires that both the client and the server have this mod installed.
- [Sodium](https://modrinth.com/mod/sodium) - This mod is client-side only, because this mod modifies the game's rendering, and the server does not have to do any rendering in the first place.
- [Xaero's Minimap](https://modrinth.com/mod/xaeros-minimap) - This mod is client-side and can be used with servers that do not have this mod installed, but installing this mod on the server can help enhance the functionality of the mod on the client side, but that will also mean the client is now required to have this mod.

**Plugins** can basically be called server-side only mods,
They can modify the core game, but not to an extent that
"mods" can do. They usually add commands and utilities to
the game, but not add new blocks. But servers with plugins
do not require that the client install anything, So it is
easy for the user to just launch up their vanilla client
and join.

### Paper
[Paper](https://papermc.io/software/paper) is a popular
Minecraft server software based on Spigot, designed to greatly
improve performance and offer more advanced features and API.

Performance is a reason why Paper is the default choice. Even
if you don't use plugins, Paper is still a great choice.

You can read Paper's documentation [here](https://docs.papermc.io/paper)

### Purpur
[Purpur](https://purpurmc.org/) is a fork of Paper, Meaning
that Purpur is based on Paper. Purpur is a drop-in replacement
for Paper servers that's designed for configurability, and new
fun and exciting gameplay features.

You can read Purpur's documentation [here](https://purpurmc.org/docs/purpur/)

### Vanilla
[Vanilla servers](https://www.minecraft.net/en-us/download/server)
simply mean the official Minecraft server software.

You can read a Minecraft wiki page covering about setting
up a Minecraft server [here](https://minecraft.wiki/w/Tutorials/Setting_up_a_server).
But it is unlikely that you'll have to read that if you're
going to use QSM.

### Fabric
[Fabric](https://fabricmc.net/) is a Minecraft mod loader
that is modular and lightweight.

When creating a Fabric server in QSM, QSM will always choose
the latest available installer version.

You can read Fabric's documentation [here](https://docs.fabricmc.net/)

### NeoForge
[NeoForge](https://neoforged.net/) is a free, open-source,
community-oriented modding API.

You can read NeoForge's documentation about how to create a mod
[here](https://docs.neoforged.net/docs/gettingstarted/)

## Proxies
**Proxies** are softwares that reroute the Minecraft game to a
different server. These are for server setups with multiple
sub-servers.

### Velocity
[Velocity](https://papermc.io/software/velocity) is a modern,
high-performance proxy software from the PaperMC team.
Designed with performance and stability in mind,
it’s a full alternative to Waterfall/BungeeCord with its own plugin ecosystem.

You can read Velocity's documentation [here](https://docs.papermc.io/velocity)
