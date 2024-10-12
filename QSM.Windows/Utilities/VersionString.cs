using System;

namespace QSM.Windows.Utilities;

class VersionString
{
    public string Major;
    public string Minor;
    public string Build;
    public string Revision;

    public VersionString(string VersionString)
    {
        string[] parts = VersionString.Split('.', StringSplitOptions.TrimEntries);
        Major = parts[0];

        if (parts.Length > 1)
            Minor = parts[1];
        
        if (parts.Length > 2)
            Build = parts[2];
        
        if (parts.Length > 3)
            Revision = parts[3];
    }
}
