﻿namespace QSM.Core.ModPluginSource
{
    public class ModPluginInfo
    {
        public required string Name;
        public required string Owner;
        public required string Slug;
        public required string IconUrl;
        public required string License;
        public string Description = string.Empty;
        public string LicenseUrl = string.Empty;
        public uint DownloadCount;
    }
}