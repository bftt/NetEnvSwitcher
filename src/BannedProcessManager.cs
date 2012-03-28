using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace NetEnvSwitcher
{
    class BannedProcessManager
    {
        static readonly string FILE_NAME = "banned_apps.cfg";
        static readonly IEqualityComparer<string> caseInsensitiveStringComparer = new CaseInsensitiveStringComparer();

        public BannedProcessManager()
        {

        }

        public IList<Process> GetBannedProcessesRunning()
        {
            ISet<string> bannedProcessNames = readBannedProcessesList();
            Process[] runningProcesses = Process.GetProcesses();

            var bannedRunning = runningProcesses.Where(p => bannedProcessNames.Contains(p.ProcessName)).ToList();
            return bannedRunning;
        }



        ISet<string> readBannedProcessesList()
        {
            HashSet<string> result = new HashSet<string>(caseInsensitiveStringComparer);

            string path = Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location),
                FILE_NAME
                );

            foreach (var line in File.ReadLines(path))
            {
                string l = line.Trim();
                if (!String.IsNullOrWhiteSpace(l))
                {
                    int c = line.IndexOf('#');
                    if (c != -1)
                    {
                        l = l.Substring(0, c).Trim();
                        if (String.IsNullOrWhiteSpace(l))
                        {
                            continue;
                        }
                    }

                    result.Add(Path.GetFileNameWithoutExtension(l));
                }
            }

            return result;
        }

        class CaseInsensitiveStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLowerInvariant().GetHashCode();
            }
        }
    }
}
