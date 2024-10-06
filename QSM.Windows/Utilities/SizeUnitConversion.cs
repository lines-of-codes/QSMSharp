namespace QSM.Windows.Utilities;

public static class SizeUnitConversion
{
    const int s_unitDistance = 1000;

    public static string bytesToAppropriateUnit(long bytes, string digitFormatting = "0.00")
    {
        if (bytes >= 1e+6)
        {
            return $"{bytesToMegabytes(bytes).ToString(digitFormatting)}MB";
        }
        if (bytes >= 1000)
        {
            return $"{bytesToKilobyte(bytes).ToString(digitFormatting)}KB";
        }

        return $"{bytes}B";
    }

    public static double bytesToMegabytes(long bytes) => bytes / (s_unitDistance * s_unitDistance);
    public static double kilobytesToGigabytes(long kb) => kb / (s_unitDistance * s_unitDistance);

    public static float bytesToKilobyte(long bytes) => bytes / s_unitDistance;
    public static float kilobytesToMegabytes(long kb) => kb / s_unitDistance;
    public static float megabytesToGigabytes(long mb) => mb / s_unitDistance;
}
