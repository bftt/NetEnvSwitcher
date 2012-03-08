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
    /// Interaction logic for AreYouSureWindow.xaml
    /// </summary>
    public partial class AreYouSureWindow : Window
    {
        public string ConfigurationName { get; private set; }

        public AreYouSureWindow(string configurationName)
        {
            ConfigurationName = configurationName;
            DataContext = this;
            InitializeComponent();
        }

        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}