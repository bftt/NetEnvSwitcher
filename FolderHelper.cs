using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NetEnvSwitcher
{
    static class FolderHelper
    {
        public static string GetAppDataPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NetEnvSwitcher");
        }
    }
}
