using System.IO;
using UnityEngine;

namespace UnityCSharp11
{
    public static class Configuration
    {
        private static bool _configLoaded;
        private static bool _enabled;

        public static bool IsEnabled
        {
            get
            {
                if (_configLoaded) return _enabled;
                _configLoaded = true;

                string configPath = Path.Combine(Application.dataPath, "..", "ProjectSettings", "Packages", "UnityCSharp11.txt");
                return _enabled = File.Exists(configPath);
            }
            set
            {
                _enabled = value;

                string packagesDir = Path.Combine(Application.dataPath, "..", "ProjectSettings", "Packages");
                if (!Directory.Exists(packagesDir)) Directory.CreateDirectory(packagesDir);

                string configPath = Path.Combine(packagesDir, "UnityCSharp11.txt");
                if (value)
                {
                    File.WriteAllText(configPath, "enabled");
                }
                else
                {
                    File.Delete(configPath);
                }
            }
        }
    }
}
