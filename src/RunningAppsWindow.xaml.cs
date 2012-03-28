using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetEnvSwitcher
{
    /// <summary>
    /// Interaction logic for RunningAppsWindow.xaml
    /// </summary>
    public partial class RunningAppsWindow : Window
    {
        public IList<System.Diagnostics.Process> Processes { get; private set; }

        public RunningAppsWindow(IList<System.Diagnostics.Process> bannedProcessesRunning)
        {
            this.Processes = bannedProcessesRunning;
            this.DataContext = this;
            this.InitializeComponent();
        }
    }
}