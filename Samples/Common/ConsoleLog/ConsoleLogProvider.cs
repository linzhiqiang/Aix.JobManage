using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ConsoleLog
{
    public class ConsoleLogProvider : ILogProvider
    {
        public ConsoleLogProvider(ConsoleLogLevel level)
        {
            ConsoleLogLogger._logLevel = level;
        }

        private ConsoleLogLogger _log = new ConsoleLogLogger();
        public ILog GetLogger(string name)
        {
            return _log;
        }
    }
}
