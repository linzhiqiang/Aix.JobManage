using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }

    public class NullLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return NullLog.Instance;
        }
    }
}
