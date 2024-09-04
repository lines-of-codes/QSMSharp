using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QSM.Windows
{
    internal class ApplicationData
    {
        public static string ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QSM");
        public static string ConfigFile = Path.Combine(ApplicationDataPath, "config.json");
        public static ApplicationConfiguration Configuration { get; set; } = new();

        public static void EnsureDataFolderExists()
        {
            Directory.CreateDirectory(ApplicationDataPath);
        }

        public static void LoadConfiguration()
        {
            if (!File.Exists(ConfigFile)) return;
            Configuration = JsonSerializer.Deserialize<ApplicationConfiguration>(File.ReadAllText(ConfigFile));
        }

        public static void SaveConfiguration()
        {
            string jsonStr = JsonSerializer.Serialize(Configuration);
            File.WriteAllText(ConfigFile, jsonStr);
        }
    }
}
