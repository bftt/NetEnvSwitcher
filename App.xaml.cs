using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;


namespace NetEnvSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            MessageBox.Show("Started");
#if !DEBUG
            if (args.Length == 1 && args[0] == "/runningAsAdmin")
#endif
            {
                MessageBox.Show("Admin");
                //moveConfigFilesToUserDirectory();

                App app = new App();
                app.InitializeComponent();

                try
                {
                    StartWindow sw = new StartWindow();
                    app.Run(sw);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "ERROR");
                }
            }
#if !DEBUG
            else
            {
                
                //moveConfigFilesToUserDirectory();

                

                try
                {
                    ProcessStartInfo proc = new ProcessStartInfo();
                    proc.UseShellExecute = true;
                    proc.WorkingDirectory = Environment.CurrentDirectory;
                    proc.FileName = typeof(App).Assembly.Location;
                    proc.Arguments = "/runningAsAdmin";
                    proc.Verb = "runas";

                    Process.Start(proc);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    // The user refused the elevation.
                    // Do nothing and return directly ...
                    return;
                }
            }
#endif
        }

        static void moveConfigFilesToUserDirectory()
        {
            string appDataFolder = FolderHelper.GetAppDataPath();
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            // Copy config files
            DirectoryInfo exeFolder = new DirectoryInfo(Path.GetDirectoryName(typeof(App).Assembly.Location));
            foreach (var fi in exeFolder.EnumerateFiles("*.cfg"))
            {
                FileInfo destination = new FileInfo(Path.Combine(appDataFolder, fi.Name));
                if (!destination.Exists)
                {
                    fi.MoveTo(destination.FullName);
                }
            }

            // Copy delete_files.cmd
            FileInfo delBatch = new FileInfo(Path.Combine(exeFolder.FullName, "delete_files.cmd"));
            if (delBatch.Exists)
            {
                FileInfo delBatchDest = new FileInfo(Path.Combine(appDataFolder, delBatch.Name));
                delBatchDest.Delete();
                delBatch.MoveTo(delBatchDest.FullName);
            }
        }

    }
}
