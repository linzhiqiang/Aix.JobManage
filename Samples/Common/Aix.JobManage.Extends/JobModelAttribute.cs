using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage.Extends
{
    public class JobModelAttribute : Attribute
    {
        public string Type { get; set; }

        public string Queue { get; set; }

        #region 工具方法
        private static ConcurrentDictionary<Type, string> JobTypeDict = new ConcurrentDictionary<Type, string>();
        private static ConcurrentDictionary<Type, string> JobQueueDict = new ConcurrentDictionary<Type, string>();

        public static string GetJobType(Type type)
        {
            if (JobTypeDict.TryGetValue(type, out string value))
            {
                return value;
            }

            var attr = GetJobModelAttrbute(type);
            string name = attr != null && !string.IsNullOrEmpty(attr.Type) ? attr.Type : type.Name;

            JobTypeDict.TryAdd(type, name);

            return name;
        }


        public static string GetJobQueue(Type type)
        {
            if (JobQueueDict.TryGetValue(type, out string value))
            {
                return value;
            }

            var attr = GetJobModelAttrbute(type);
            string name = attr != null && !string.IsNullOrEmpty(attr.Queue) ? attr.Queue : null;

            JobQueueDict.TryAdd(type, name);

            return name;
        }


        private static JobModelAttribute GetJobModelAttrbute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(JobModelAttribute), true);
            return attrs != null && attrs.Length > 0 ? attrs[0] as JobModelAttribute : null;
        }

        #endregion

    }
}
