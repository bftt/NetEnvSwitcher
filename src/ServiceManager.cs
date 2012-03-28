using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ServiceProcess;

namespace NetEnvSwitcher
{
    class ServiceWrapper
    {
        readonly ServiceController _controller;
        readonly TimeSpan _timeOut;
        readonly string _displayName;

        public ServiceWrapper(string name, bool isOptional, TimeSpan timeout)
        {
            _controller = new ServiceController(name);

            try
            {
                var status = _controller.Status;
                // Dummy logic
                if (status == ServiceControllerStatus.Running)
                {
                    status = ServiceControllerStatus.Stopped;
                }
            }
            catch (InvalidOperationException)
            {
                _controller = null;
                if (!isOptional)
                {
                    throw;
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string Status
        {
            get
            {
                string result = "Unknown";

                if (_controller != null)
                { 
                    _controller.Refresh();
                    result = _controller.Status.ToString();
                }

                return result;
            }
        }

        public void Start()
        {
            if (_controller != null)
            {
                _controller.Refresh();
                if (_controller.Status != ServiceControllerStatus.Running &&
                    _controller.Status != ServiceControllerStatus.StartPending)
                {
                    if (_controller.Status != ServiceControllerStatus.StartPending)
                    {
                        _controller.Start();
                    }
                    _controller.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, _timeOut);
                }
            }
        }

        public void Stop()
        {
            if (_controller != null)
            {
                _controller.Refresh();
                if (_controller.Status != ServiceControllerStatus.Stopped &&
                    _controller.Status != ServiceControllerStatus.StopPending)
                {
                    if (_controller.Status != ServiceControllerStatus.StopPending)
                    {
                        _controller.Stop();
                    }
                    _controller.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, _timeOut);
                }
            }
        }

    }

    public class ServiceManager
    {
        readonly ILogger _logger;
        readonly ServiceWrapper _guardServerController;
        readonly ServiceWrapper _guardianController;
        readonly ServiceWrapper _ttmController;


        public ServiceManager(ILogger logger) : this(logger, TimeSpan.FromSeconds(20))
        {
        }

        public ServiceManager(ILogger logger, TimeSpan serviceWaitTimeout)
        {
            _logger = logger;

            _guardServerController = new ServiceWrapper("TT Guardian Server", true, serviceWaitTimeout);
            _guardianController    = new ServiceWrapper("TT Guardian", false, serviceWaitTimeout);
            _ttmController         = new ServiceWrapper("TT Messaging", false, serviceWaitTimeout);
        }

        public string GuardServerStatus
        {
            get
            {
                return _guardServerController.Status;
            }
        }

        public string GuardianStatus
        {
            get
            {
                return _guardianController.Status;
            }
        }

        public string TtmStatus
        {
            get
            {
                return _ttmController.Status;
            }
        }

        public void StartServices(bool startGuardServer)
        {
            startService(_ttmController);
            startService(_guardianController);

            if (startGuardServer)
            {
                startService(_guardServerController);
            }
        }

        public void StopServices()
        {
            stopService(_guardServerController);
            stopService(_guardianController);
            stopService(_ttmController);
        }

        void startService(ServiceWrapper service)
        {
            _logger.WriteLine("Starting {0} service", service.DisplayName);
            service.Start();
        }

        void stopService(ServiceWrapper service)
        {
            _logger.WriteLine("Stopping {0} service", service.DisplayName);
            service.Stop();
        }
    }
}
