using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ClickOnce
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string EXE_FILE_NAME = "NetEnvSwitcher.exe";

            string folder = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = Path.Combine(folder, EXE_FILE_NAME);
            proc.Verb = "runas";

            try
            {
                Process.Start(proc);
            }
            catch
            {
                // The user refused the elevation.
                // Do nothing and return directly ...
                return;
            }
        }
    }
}
