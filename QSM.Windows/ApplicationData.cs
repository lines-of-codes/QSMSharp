using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Windows
{
    internal class ApplicationData
    {
        public static string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\QSM";

        public static void EnsureDataFolderExists()
        {
            Directory.CreateDirectory(ApplicationDataPath);
        }
    }
}
