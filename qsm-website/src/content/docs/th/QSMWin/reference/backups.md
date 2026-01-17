---
title: Backups
description: All about backups in QSM.
---

Backups are a copy of data at a certain point in time,
They are made to make sure if a server fails, it can be
restored back to its past state.

Backups in QSM (Windows) can be compressed to either
`.zip` or `.tar` files. The default recommended option is
`.tar.zst` ([tar](https://en.wikipedia.org/wiki/Tar_(computing)) archives with [Zstandard
compression](https://en.wikipedia.org/wiki/Zstd))

All available compression methods include:
- None
- Deflate/GZip
- BZip2
- LZMA
- PPMd (only supported by zip archives)
- Zstd (only supported by tar archives)

QSM (try to) help you follow the 3-2-1 backup rule.

QSM plans to offer the option of backing up to these cloud
storage providers:

- OneDrive
- Dropbox
- Google Drive

(The list is sorted according to what will be implemented
first)

Obviously, you can also manually upload the backups to another
cloud provider.

Automatic backups will be implemented eventually, after all
the basic features are completed.
