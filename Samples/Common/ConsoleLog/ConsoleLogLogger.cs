using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ConsoleLog
{
    public enum ConsoleLogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class ConsoleLogLogger : ILog
    {
        internal static  ConsoleLogLevel _logLevel = ConsoleLogLevel.Trace;
       
        public void Debug(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Debug, message, exception);
        }

        public void Error(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Error, message, exception);
        }

        public void Fatal(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Fatal, message, exception);
        }

        public void Info(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Info, message, exception);
        }

        public void Trace(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Trace, message, exception);
        }

        public void Warn(string message, Exception exception = null)
        {
            Log(ConsoleLogLevel.Warn, message, exception);
        }

        private void Log(ConsoleLogLevel level, string message, Exception exception = null)
        {
            if (IsEnabled(level))
            {
                string  exceptionMsg = string.Empty;
                if (exception != null)
                {
                    exceptionMsg = $", {exception.Message},{exception.StackTrace}";
                }

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}[{level.ToString()}], {message}{exceptionMsg}");
            }
        }

        private bool IsEnabled(ConsoleLogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }
    }
}
