using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage.Logging
{
    public interface ILog
    {
        void Trace(string message, Exception exception = null);

        void Debug(string message, Exception exception = null);

        void Info(string message, Exception exception = null);

        void Warn(string message, Exception exception = null);

        void Error(string message, Exception exception = null);

        void Fatal(string message, Exception exception = null);
    }

    public class NullLog : ILog
    {
        public static NullLog Instance = new NullLog();
        private NullLog() { }
        public void Trace(string message, Exception exception = null)
        {
        }

        public void Debug(string message, Exception exception = null)
        {
        }

        public void Info(string message, Exception exception = null)
        {
        }

        public void Warn(string message, Exception exception = null)
        {
        }


        public void Error(string message, Exception exception = null)
        {
        }

        public void Fatal(string message, Exception exception = null)
        {
        }


    }
}
