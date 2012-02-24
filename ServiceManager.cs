﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ServiceProcess;

namespace NetEvnSwitcher
{
    public class ServiceManager
    {
        readonly ILogger _logger;
        readonly TimeSpan _serviceWaitTimeout;
        readonly ServiceController _guardServerController;
        readonly ServiceController _guardianController;
        readonly ServiceController _ttmController;


        public ServiceManager(ILogger logger) : this(logger, TimeSpan.FromSeconds(20))
        {
        }

        public ServiceManager(ILogger logger, TimeSpan serviceWaitTimeout)
        {
            _logger = logger;

            _serviceWaitTimeout    = serviceWaitTimeout;
            _guardServerController = new ServiceController("TT Guardian Server");
            _guardianController    = new ServiceController("TT Guardian");
            _ttmController         = new ServiceController("TT Messaging");
        }

        public ServiceControllerStatus GuardServerStatus
        {
            get
            {
                _guardServerController.Refresh();
                return _guardServerController.Status;
            }
        }

        public ServiceControllerStatus GuardianStatus
        {
            get
            {
                _guardianController.Refresh();
                return _guardianController.Status;
            }
        }

        public ServiceControllerStatus TtmStatus
        {
            get
            {
                _ttmController.Refresh();
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

        void startService(ServiceController service)
        {
            _logger.WriteLine("Starting {0} service", service.DisplayName);

            service.Refresh();
            if (service.Status != ServiceControllerStatus.Running &&
                service.Status != ServiceControllerStatus.StartPending)
            {
                if (service.Status != ServiceControllerStatus.StartPending)
                {
                    service.Start();
                }
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, _serviceWaitTimeout);
            }
        }

        void stopService(ServiceController service)
        {
            _logger.WriteLine("Stopping {0} service", service.DisplayName);

            service.Refresh();
            if (service.Status != ServiceControllerStatus.Stopped &&
                service.Status != ServiceControllerStatus.StopPending)
            {
                if (service.Status != ServiceControllerStatus.StopPending)
                {
                    service.Stop();
                }
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, _serviceWaitTimeout);
            }
        }
    }
}
