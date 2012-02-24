using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NetEnvSwitcher
{
    public class EnvironmentConfig
    {
        public string Name { get; set; }
        public FileInfo FileInfo { get; set; }
        public bool IsGuardServerAllowed { get; set; }

        public string Multicast { get; set; }
        public string RemoteDaemon { get; set; }
    }
}
