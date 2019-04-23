using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.NLogLog
{
    public class NLogLogger : ILog
    {
        private NLog.ILogger _nLogger;
        public NLogLogger(NLog.ILogger nLogger)
        {
            _nLogger = nLogger;
        }
        public void Debug(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Debug(message);
            }
            else
            {
                _nLogger.Debug(exception, message);
            }


        }

        public void Error(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Error(message);
            }
            else
            {
                _nLogger.Error(exception, message);
            }
        }

        public void Fatal(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Fatal(message);
            }
            else
            {
                _nLogger.Fatal(exception, message);
            }
        }

        public void Info(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Info(message);
            }
            else
            {
                _nLogger.Info(exception, message);
            }
        }

        public void Trace(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Trace(message);
            }
            else
            {
                _nLogger.Trace(exception, message);
            }
        }

        public void Warn(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _nLogger.Warn(message);
            }
            else
            {
                _nLogger.Warn(exception, message);
            }
        }
    }
}
