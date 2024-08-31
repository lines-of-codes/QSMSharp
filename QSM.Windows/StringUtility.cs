using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QSM.Windows
{
    internal partial class StringUtility
    {
        static Regex invalidFileNameCheck = InvalidFileNameCheck();

        public static string TurnIntoValidFileName(string original)
        {
            string result = invalidFileNameCheck.Replace(original, "");

            if (result.Length == 0)
            {
                return Path.GetRandomFileName();
            }

            return result;
        }

        [GeneratedRegex("[\\\\/<>:|\\\\?\\\\*\\\"\\0]|^(PRN|AUX|NUL|CON)$|^(COM|LPT)[\\d¹²³]{1}$|[ .]$")]
        private static partial Regex InvalidFileNameCheck();
    }
}
