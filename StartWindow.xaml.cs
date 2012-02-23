using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

namespace NetEvnSwitcher
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window, ILogger, INotifyPropertyChanged
    {
        readonly EnvironmentConfigManager _envManager;
        readonly ServiceManager _serviceManager;
        readonly BannedProcessManager _bannedProcManager;
        readonly DispatcherTimer _timer;

        IList<EnvironmentConfig> _environments = null;

        public StartWindow()
        {
            DataContext = this;
            this.Loaded += new RoutedEventHandler(StartWindow_Loaded);

            _envManager = new EnvironmentConfigManager(this);
            _serviceManager = new ServiceManager(this);
            _bannedProcManager = new BannedProcessManager();

            _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            _timer.Interval = TimeSpan.FromSeconds(60);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();

            InitializeComponent();
            setupConfigurationButtons();
            getServerStatuses();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            getServerStatuses();
        }

        private void getServerStatuses()
        {
            GuardServerStatus = getServerStatus(() => _serviceManager.GuardServerStatus);
            GuardianStatus    = getServerStatus(() => _serviceManager.GuardianStatus);
            TtmStatus         = getServerStatus(() => _serviceManager.TtmStatus);

            try
            {
                var currentEnv = _envManager.GetCurrentEnvironment();
                if (!String.IsNullOrEmpty(currentEnv.RemoteDaemon))
                {
                    var f = _environments.FirstOrDefault(e => e.RemoteDaemon == currentEnv.RemoteDaemon);
                    if (f != null)
                    {
                        // Matched by remote daemon
                        CurrentEnvironment = f.Name;
                    }
                    else
                    {
                        CurrentEnvironment = "rem: " + currentEnv.RemoteDaemon;
                    }
                }
                else if (!String.IsNullOrEmpty(currentEnv.Multicast))
                {
                    var f = _environments.FirstOrDefault(e => e.Multicast == currentEnv.Multicast);
                    if (f != null)
                    {
                        // matched by multicast
                        CurrentEnvironment = f.Name;
                    }
                    else
                    {
                        CurrentEnvironment = currentEnv.Multicast;
                    }
                }
                else
                {
                    CurrentEnvironment = "?";
                }
            }
            catch
            {
                CurrentEnvironment = "<problem>";
            }
        }

        string getServerStatus(Func<object> statusFetcher)
        {
            string result = String.Empty;
            try
            {
                result = statusFetcher().ToString();
            }
            catch (Exception ex)
            {
                result = "Problem";

                WriteLine("Problem reading status");
                while (ex != null)
                {
                    WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }

            return result;
        }

        void StartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogExpander.IsExpanded = false;
        }

        void setupConfigurationButtons()
        {
            _environments = _envManager.GetEnvironemtnts();

            ConfigurationsPanel.AddHandler(Button.ClickEvent, new RoutedEventHandler(configButtonClicked));
            ConfigurationsPanel.Children.Clear();

            foreach (var c in _environments)
            {
                ContentPresenter b = new ContentPresenter();
                b.Content = c;
                b.Tag = c;

                ConfigurationsPanel.Children.Add(b);
            }
        }

        void configButtonClicked(object sender, RoutedEventArgs e)
        {
            ContentPresenter cp = e.Source as ContentPresenter;
            if (cp != null)
            {
                e.Handled = true;
                EnvironmentConfig config = (EnvironmentConfig)cp.Content;

                switchConfigurations(config);
            }
        }

        private void switchConfigurations(EnvironmentConfig config)
        {
            var w = new NetEnvSwitcher.AreYouSureWindow();
            w.Owner = this;
            w.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            if (w.ShowDialog() == true)
            {
                ConfigurationsPanel.IsEnabled = false;

                var t = new System.Threading.Tasks.Task(() =>
                    {
                        try
                        {
                            WriteLine("======== Switching configuration to " + config.Name);

                            var bannedProcessesRunning = _bannedProcManager.GetBannedProcessesRunning();
                            if (bannedProcessesRunning.Count == 0)
                            {

                                _serviceManager.StopServices();

                                _envManager.SwitchTo(config);

                                _envManager.ResetAConfigRevisionCount();

                                WriteLine("Sleeping for 3 seconds for good measure...");
                                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));

                                _serviceManager.StartServices(config.IsGuardServerAllowed);

                                WriteLine("======== Finished switching configuration to " + config.Name);
                            }
                            else
                            {
                                WriteLine("PROBLEM: You need to stop apps before switching!");
                                foreach (var p in bannedProcessesRunning)
                                {
                                    WriteLine(" -> " + p.ProcessName);
                                }

                                Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        var window = new NetEnvSwitcher.RunningAppsWindow(bannedProcessesRunning);
                                        window.Owner = this;
                                        window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                        window.ShowDialog();
                                    }));
                            }

                            refreshGuiAfterOperation();
                        }
                        catch (Exception ex)
                        {
                            WriteLine("!!!!!!!! Problem switching configuration to " + config.Name);

                            while (ex != null)
                            {
                                WriteLine(ex.ToString());
                                ex = ex.InnerException;
                            }

                            refreshGuiAfterOperation();
                        }
                    });
                t.Start();
            }
        }

        void refreshGuiAfterOperation()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConfigurationsPanel.IsEnabled = true;
                getServerStatuses();
            }));
        }

        RandomPastelColorGenerator _gen = new RandomPastelColorGenerator();

        public void WriteLine(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    var span = new Span();
                    span.Inlines.Add(String.Format("{0} : {1}", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"), message));
                    span.Foreground = _gen.GetNextBrush();

                    LogParagraph.Inlines.Add(span);
                    LogParagraph.Inlines.Add(new LineBreak());

                    FindScrollViewer(LogDocViewer).ScrollToEnd();
                }),
                System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void Expander_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            Expander exp = (Expander)sender;
            FrameworkElement content = exp.Content as FrameworkElement;
            if (content != null)
            {
                double width = _previousExpanderWidth > 0 ? _previousExpanderWidth : 510;
                this.Width += _previousExpanderWidth;
            }
        }

        private double _previousExpanderWidth = 510.0;
        private void Expander_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            Expander exp = (Expander)sender;
            FrameworkElement content = exp.Content as FrameworkElement;
            if (content != null)
            {
                _previousExpanderWidth = content.ActualWidth;
                this.Width -= _previousExpanderWidth;
            }
        }


        private string m_GuardServerStatus;
        public string GuardServerStatus
        {
            get
            {
                return m_GuardServerStatus;
            }
            set
            {
                if (m_GuardServerStatus != value)
                {
                    m_GuardServerStatus = value;
                    OnPropertyChanged("GuardServerStatus");
                }
            }
        }

        private string m_GuardianStatus;
        public string GuardianStatus
        {
            get
            {
                return m_GuardianStatus;
            }
            set
            {
                if (m_GuardianStatus != value)
                {
                    m_GuardianStatus = value;
                    OnPropertyChanged("GuardianStatus");
                }
            }
        }

        private string m_TtmStatus;
        public string TtmStatus
        {
            get
            {
                return m_TtmStatus;
            }
            set
            {
                if (m_TtmStatus != value)
                {
                    m_TtmStatus = value;
                    OnPropertyChanged("TtmStatus");
                }
            }
        }

        private string m_CurrentEnvironment;
        public string CurrentEnvironment
        {
            get
            {
                return m_CurrentEnvironment;
            }
            set
            {
                if (m_CurrentEnvironment != value)
                {
                    m_CurrentEnvironment = value;
                    OnPropertyChanged("CurrentEnvironment");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        static ScrollViewer FindScrollViewer(FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }

            // Border is the first child of first child of a ScrolldocumentViewer
            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return null;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return null;
            }

            return border.Child as ScrollViewer;
        }
    }

}