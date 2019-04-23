using NCrontab;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
    public static class JobHelper
    {
        public static string DefaultQueue = "defaultqueue";

        public static CrontabSchedule ParseCron(string cron)
        {
            var options = new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = cron.Split(' ').Length > 5,
            };
            return CrontabSchedule.Parse(cron, options);
        }

        public static TimeSpan GetNextDueTime(CrontabSchedule Schedule, DateTime LastDueTime, DateTime now)
        {
            var nextOccurrence = Schedule.GetNextOccurrence(LastDueTime);
            TimeSpan dueTime = nextOccurrence - now;// DateTime.Now;

            if (dueTime.TotalMilliseconds <= 0)
            {
                dueTime = TimeSpan.Zero;
            }

            return dueTime;
        }

        /// <summary>
        /// 随机一个数组
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IList<string> RandomArray(string[] list)
        {
            SortedList<string, string> result = new SortedList<string, string>();
            if (list == null) return result.Values;
            foreach (var item in list)
            {
                //var temp = new Random(Guid.NewGuid().GetHashCode()).Next(1, max);
                result.Add(Guid.NewGuid().ToString(), item);
            }
            return result.Values;
        }
    }
}
