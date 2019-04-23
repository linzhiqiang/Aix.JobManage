using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.NLogLog
{
    public class NLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(name));
        }
    }
}
