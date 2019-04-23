using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Aix.JobManage.Logging
{
    public static class LogProviderFactory
    {
        private static ILogProvider _currentLogProvider;

        private static NullLogProvider DefaultLogProvider = new NullLogProvider();

        public static void SetCurrentLogProvider(ILogProvider logProvider)
        {
            //Volatile.Write(ref _currentLogProvider, logProvider);
            _currentLogProvider = logProvider;
        }

        public static ILog GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public static ILog GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            return GetLogger(stackFrame.GetMethod().DeclaringType);
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static ILog GetLogger(string name)
        {
            //ILogProvider logProvider = Volatile.Read(ref _currentLogProvider) ?? DefaultLogProvider;
            ILogProvider logProvider = _currentLogProvider ?? DefaultLogProvider;
            return logProvider.GetLogger(name);
        }

    }
}
