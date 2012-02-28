using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;

namespace NetEnvSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 1)
            {
                return runConsole(args[0]);
            }
            else
            {
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
                    return 1;
                }

                return 0;
            }
        }

        static int runConsole(string configName)
        {
            int result = 0;
            try
            {
                var logger = new Win32ConsoleLogger();
                ServiceManager serviceManager = new ServiceManager(logger);
                EnvironmentConfigManager envManager = new EnvironmentConfigManager(logger);
                
                var allConfigs = envManager.GetEnvironemtnts();
                var config = allConfigs.First(c => c.Name.Equals(configName, StringComparison.CurrentCultureIgnoreCase));

                logger.WriteLine("======== Switching configuration to " + config.Name);

                serviceManager.StopServices();

                envManager.SwitchTo(config);
                
                envManager.ResetAConfigRevisionCount();

                logger.WriteLine("Sleeping for 5 seconds for good measure...");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

                serviceManager.StartServices(config.IsGuardServerAllowed);
                logger.WriteLine("======== Finished switching configuration to " + config.Name);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("========= ERROR =======================================");
                while (ex != null)
                {
                    Console.Error.WriteLine(ex.ToString());
                    ex = ex.InnerException;
                }
                result = 1;
            }
            return result;
        }

        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll",
            EntryPoint = "FreeConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll",
            EntryPoint = "AttachConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(uint nStdHandle);

        class Win32ConsoleLogger : ILogger
        {
            IntPtr _console;
            System.IO.FileStream _fs;
            System.IO.StreamWriter _writer;


            public Win32ConsoleLogger()
            {
                uint STD_OUTPUT_HANDLE = 0xfffffff5;
                _console = GetStdHandle(STD_OUTPUT_HANDLE);
                _fs = new System.IO.FileStream(_console, System.IO.FileAccess.Write);
                _writer = new System.IO.StreamWriter(_fs);
                Console.SetOut(_writer);
            }

            public void WriteLine(string message)
            {
                Console.WriteLine(message);
            }
        }

    }



}
