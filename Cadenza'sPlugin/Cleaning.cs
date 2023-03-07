using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadenza_sPlugin
{
    internal class Cleaning
    {
        public static void Clean()
        {
            var path = Path.Combine(BepInEx.Paths.PluginPath, "Dump", "UIText.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var path2 = Path.Combine(BepInEx.Paths.PluginPath, "Dump", "TA.txt");
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
            var path3 = Path.Combine(BepInEx.Paths.PluginPath, "Dump", "Hardcoded.txt");
            if (File.Exists(path3))
            {
                File.Delete(path3);
            }
        }
    }
}
