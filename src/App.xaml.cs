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



}
