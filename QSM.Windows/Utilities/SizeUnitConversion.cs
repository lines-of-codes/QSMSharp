using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Windows.Utilities
{
    public static class SizeUnitConversion
    {
        public static string bytesToAppropriateUnit(long bytes, string digitFormatting = "0.00")
        {
            if (bytes >= 1e+6)
            {
                return $"{bytesToMegabytes(bytes).ToString(digitFormatting)}MB";
            }
            else if (bytes >= 1000)
            {
                return $"{bytesToKilobyte(bytes).ToString(digitFormatting)}KB";
            }

            return $"{bytes}B";
        }

        public static double bytesToMegabytes(long bytes)
        {
            return bytes / 1e+6;
        }

        public static float bytesToKilobyte(long bytes)
        {
            return bytes / 1000;
        }
    }
}
