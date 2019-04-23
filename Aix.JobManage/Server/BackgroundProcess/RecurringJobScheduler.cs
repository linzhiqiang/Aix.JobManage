using Aix.JobManage.Logging;
using Aix.JobManage.Utils;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    /// <summary>
    /// 定时任务处理
    /// </summary>
    public class RecurringJobScheduler : IBackgroundProcess
    {
        private ILog _log = LogProviderFactory.GetLogger<RecurringJobScheduler>();
        private BackgroundJobServerOptions _options;
        public IJobStorage _jobStorage;

        public RecurringJobScheduler(IJobStorage jobStorage, BackgroundJobServerOptions options)
        {
            _jobStorage = jobStorage;
            _options = options;
        }

        public async Task Execute(BackgroundProcessContext context)
        {
            List<double> nextExecuteDelays = new List<double>(); //记录每个任务的下次执行时间，取最小的等待
            var lockKey = "recurringjob:lock";

            await _jobStorage.Lock(lockKey, TimeSpan.FromMinutes(1), async () =>
            {
                var list = await _jobStorage.GetAllRecurringJobId();
                foreach (var jobId in list)
                {
                    if (context.IsShutdownRequested) return;
                    var now = DateTime.Now;
                    var jobData = await _jobStorage.GetRecurringJobData(jobId);
                    if (jobData == null) continue;

                    var lastExecuteTime = now;
                    if (string.IsNullOrEmpty(jobData.LastExecuteTime))
                    {
                        await _jobStorage.SetRecurringJobExecuteTime(jobId, DateUtils.GetTimeStamp(now));
                    }
                    else
                    {
                        lastExecuteTime = DateUtils.TimeStampToDateTime(long.Parse(jobData.LastExecuteTime));
                    }

                    var Schedule = JobHelper.ParseCron(jobData.Cron);
                    var nextExecuteTimeSpan = JobHelper.GetNextDueTime(Schedule, lastExecuteTime, now);
                    if (nextExecuteTimeSpan.TotalMilliseconds <= 0)
                    {
                        var executeTime = DateTime.Now;
                        await _jobStorage.SetRecurringJobExecuteTime(jobId, DateUtils.GetTimeStamp(executeTime));

                        await BackgroundJobClient.Instance.AddJob(jobData.Data, jobData.Queue);//插入普通任务队列即可

                        nextExecuteTimeSpan = JobHelper.GetNextDueTime(Schedule, executeTime, DateTime.Now);
                    }
                    nextExecuteDelays.Add(nextExecuteTimeSpan.TotalMilliseconds);
                }
            },()=>Task.CompletedTask);

            var minValue = nextExecuteDelays.Any() ? (int)nextExecuteDelays.Min() : _options.RecurringJobTimeout.TotalMilliseconds;
            var delay = Math.Max(minValue, 1000); //应该延时这么久最好

            _jobStorage.WaitForRecurringJob(TimeSpan.FromMilliseconds(delay), context.CancellationToken);
        }



    }
}
