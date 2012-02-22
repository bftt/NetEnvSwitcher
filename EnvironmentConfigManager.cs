using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NetEvnSwitcher
{
    public class EnvironmentConfigManager
    {
        readonly DirectoryInfo _myDir;
        readonly FileInfo _ttmConfig;
        readonly ILogger _logger;

        public EnvironmentConfigManager(ILogger logger)
        {
            _logger = logger;

            readRegistry();

            _myDir = new DirectoryInfo(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location));
            _ttmConfig = new FileInfo(Path.Combine(_configDirectory, "ttmd.cfg"));
        }

        public IList<EnvironmentConfig> GetEnvironemtnts()
        {
            List<EnvironmentConfig> result = new List<EnvironmentConfig>();

            var files = _myDir.EnumerateFiles("*.ttmd.cfg");
            foreach (var fileInfo in files)
            {
                EnvironmentConfig cfg = getConfigFromFile(fileInfo);

                result.Add(cfg);
            }

            return result;
        }

        public EnvironmentConfig GetCurrentEnvironment()
        {
            return getConfigFromFile(_ttmConfig);
        }

        private static EnvironmentConfig getConfigFromFile(FileInfo fileInfo)
        {
            EnvironmentConfig cfg = new EnvironmentConfig();
            cfg.FileInfo = fileInfo;
            cfg.IsGuardServerAllowed = !System.Text.RegularExpressions.Regex.IsMatch(fileInfo.Name, "NoGS", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cfg.Name = System.Text.RegularExpressions.Regex.Replace(fileInfo.Name, @"\.ttmd\.cfg", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cfg.Name = System.Text.RegularExpressions.Regex.Replace(cfg.Name, @"\.NoGS", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);


            using (var reader = fileInfo.OpenRead())
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(reader);

                var xpathNavi = doc.CreateNavigator();
                var multicastNode = xpathNavi.SelectSingleNode("/ttmconfiguration[1]/multicastgroups[1]");
                if (multicastNode != null && !String.IsNullOrEmpty(multicastNode.Value))
                {
                    cfg.Multicast = multicastNode.Value.Replace("> =", String.Empty).Trim();
                }

                var daemonNode = xpathNavi.SelectSingleNode("/ttmconfiguration[1]/proxy[1]/daemon1[1]");
                if (daemonNode != null && !String.IsNullOrEmpty(daemonNode.Value))
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"\d+\.\d+\.\d+\.\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                    var match = regex.Match(daemonNode.Value);

                    if (match.Success)
                    {
                        cfg.RemoteDaemon = match.Value;
                        cfg.Multicast = null;
                    }
                }
            }
            return cfg;
        }


        public void SwitchTo(EnvironmentConfig config)
        {
            _logger.WriteLine("Executing batch to delete config files");
            deleteFiles();


            _logger.WriteLine("Copying ttmd.cfg file");
            config.FileInfo.CopyTo(_ttmConfig.FullName, true);
        }

        private void deleteFiles()
        {
            var psi = new System.Diagnostics.ProcessStartInfo(Path.Combine(_myDir.FullName, "delete_files.cmd"));
            psi.EnvironmentVariables.Add("DATFILES_DIR", _dataDirectory);
            psi.EnvironmentVariables.Add("INSTALL_DIR", _installDirectory);

            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.ErrorDialog = false;

            var errorWH = new System.Threading.ManualResetEvent(false);
            var outputWH = new System.Threading.ManualResetEvent(false);


            using (System.Diagnostics.Process p = new System.Diagnostics.Process())
            {
                p.StartInfo = psi;

                p.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWH.Set();
                        }
                        else
                        {
                            _logger.WriteLine(e.Data);
                        }
                    };

                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWH.Set();
                    }
                    else
                    {
                        _logger.WriteLine(e.Data);
                    }
                };

                p.Start();

                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                p.WaitForExit();

                errorWH.WaitOne();
                outputWH.WaitOne();

                if (p.ExitCode != 0)
                {
                    throw new Exception("Batch file returned with exit code of " + p.ExitCode);
                }
            }
        }


        public void ResetAConfigRevisionCount()
        {
            _logger.WriteLine("Setting AConfig revision count to zero");
            resetRevisionCount(_aConfigFilePath, 0);
        }

        private static void resetRevisionCount(string aconfigFile, int revisionCount)
        {
            const string XPATH = @"config-values/config-value[@dn='AConfig.Meta.RevisionCount']/value";

            XDocument dom = XDocument.Load(aconfigFile);
            var node = dom.Root.XPathSelectElement(XPATH);

            if (node != null)
            {
                node.Value = revisionCount.ToString();
            }

            dom.Save(aconfigFile);
        }


        private string _installDirectory = null;
        private string _configDirectory = null;
        private string _dataDirectory = null;
        private string _aConfigFilePath  = null;

        /// <summary>
        /// Reads registry to get path to core's configuration files, which contain
        /// paths to log directory and TT config directory.
        /// </summary>
        private void readRegistry()
        {
            /// <summary>
            /// Guardian Registry key to TT's core configuration.
            /// </summary>
            const string CONFIG_DIR_REG_KEY = @"HKEY_LOCAL_MACHINE\Software\Trading Technologies\AConfig";

            /// <summary>
            /// Guardian Registry value specifying TT's config directory.
            /// </summary>
            const string CONFIG_DIR_REG_VAL = "FileLocation";

            /// <summary>
            /// Installation Registry key to TT's core configuration.
            /// </summary>
            const string INSTALLED_CONFIG_DIR_REG_KEY = @"HKEY_LOCAL_MACHINE\Software\Trading Technologies\Installation";

            /// <summary>
            /// Installation Registry value specifying TT's config directory.
            /// </summary>
            const string INSTALLED_CONFIG_DIR_REG_VAL = "CONFIGFILEDIR";

            const string INSTALLED_DATFILE_DIR_REG_VAL = "DATFILEDIR";
            const string INSTALLED_INSTALLROOT_DIR_REG_VAL = "INSTALLROOT";

            // Read config dir from installed reg
            _configDirectory = (string)Microsoft.Win32.Registry.GetValue(INSTALLED_CONFIG_DIR_REG_KEY, INSTALLED_CONFIG_DIR_REG_VAL, null);

            _dataDirectory = (string)Microsoft.Win32.Registry.GetValue(INSTALLED_CONFIG_DIR_REG_KEY, INSTALLED_DATFILE_DIR_REG_VAL, null);

            _installDirectory = (string)Microsoft.Win32.Registry.GetValue(INSTALLED_CONFIG_DIR_REG_KEY, INSTALLED_INSTALLROOT_DIR_REG_VAL, null);

            // Make sure we actually read a dir from registry
            if (_configDirectory == null)
            {
                // If installed reg doesn't exist then Read config dir from guardian reg
                _configDirectory = (string)Microsoft.Win32.Registry.GetValue(CONFIG_DIR_REG_KEY, CONFIG_DIR_REG_VAL, null);

                // Make sure we actually read a dir from registry
                if (_configDirectory == null)
                    throw new ApplicationException("Could not get path to AConfig.xml from the registry");
            }

            _aConfigFilePath = System.IO.Path.Combine(_configDirectory, "AConfig.xml");
        }
    }
}
