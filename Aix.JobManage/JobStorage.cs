using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
   public class JobStorage
    {
        private static readonly object LockObject = new object();
        private static IJobStorage _current;

        public static IJobStorage Current
        {
            get
            {
                lock (LockObject)
                {
                    return _current;
                }
            }
            set
            {
                lock (LockObject)
                {
                    _current = value;
                }
            }
        }

    }
}
