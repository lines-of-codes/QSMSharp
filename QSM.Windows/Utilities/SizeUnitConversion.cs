using System;

namespace QSM.Windows.Utilities;

public static class SizeUnitConversion
{
	const int MetricUnitDistance = 1000;
	public readonly static double Gigabyte = Math.Pow(MetricUnitDistance, 3);

	public static string BytesToAppropriateUnit(long bytes, string digitFormatting = "0.00")
	{
		if (bytes >= Gigabyte)
			return $"{BytesToGigabyte(bytes).ToString(digitFormatting)}GB";

		if (bytes >= 1e+6)
			return $"{BytesToMegabytes(bytes).ToString(digitFormatting)}MB";

		if (bytes >= 1000)
			return $"{BytesToKilobyte(bytes).ToString(digitFormatting)}kB";

		return $"{bytes}B";
	}

	public static double BytesToGigabyte(double bytes) => bytes / Gigabyte;

	public static float BytesToMegabytes(float bytes) => bytes / (MetricUnitDistance * MetricUnitDistance);
	public static float KilobytesToGigabytes(float kb) => kb / (MetricUnitDistance * MetricUnitDistance);

	public static float BytesToKilobyte(float bytes) => bytes / MetricUnitDistance;
	public static float KilobytesToMegabytes(float kb) => kb / MetricUnitDistance;
	public static float MegabytesToGigabytes(float mb) => mb / MetricUnitDistance;
}
