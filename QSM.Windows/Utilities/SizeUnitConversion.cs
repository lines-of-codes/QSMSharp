namespace QSM.Windows.Utilities;

public static class SizeUnitConversion
{
	const int UnitDistance = 1000;

	public static string bytesToAppropriateUnit(long bytes, string digitFormatting = "0.00")
	{
		if (bytes >= 1e+6)
		{
			return $"{bytesToMegabytes(bytes).ToString(digitFormatting)}MB";
		}
		if (bytes >= 1000)
		{
			return $"{bytesToKilobyte(bytes).ToString(digitFormatting)}kB";
		}

		return $"{bytes}B";
	}

	public static float bytesToMegabytes(float bytes) => bytes / (UnitDistance * UnitDistance);
	public static float kilobytesToGigabytes(float kb) => kb / (UnitDistance * UnitDistance);

	public static float bytesToKilobyte(float bytes) => bytes / UnitDistance;
	public static float kilobytesToMegabytes(float kb) => kb / UnitDistance;
	public static float megabytesToGigabytes(float mb) => mb / UnitDistance;
}
