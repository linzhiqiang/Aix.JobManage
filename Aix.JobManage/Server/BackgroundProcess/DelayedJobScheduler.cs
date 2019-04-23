using Aix.JobManage.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Aix.JobManage.Logging;

namespace Aix.JobManage.Server
{
    /// <summary>
    /// 延迟任务处理
    /// </summary>
    public class DelayedJobScheduler : IBackgroundProcess
    {
        private ILog _log = LogProviderFactory.GetLogger<DelayedJobScheduler>();
        public IJobStorage _jobStorage;
        private BackgroundJobServerOptions _options;
        public DelayedJobScheduler(IJobStorage jobStorage, BackgroundJobServerOptions options)
        {
            _jobStorage = jobStorage;
            _options = options;
        }


        public async Task Execute(BackgroundProcessContext context)
        {
            var lockKey = "delay:lock";
            int delay = 0;
            await _jobStorage.Lock(lockKey, TimeSpan.FromMinutes(1), async () =>
            {
                double delayMillSecond = 60 * 1000;  //多查询一分钟的数据，便于delay
                double diffMillSecond = 0;
                var now = DateTime.Now;
                var maxScore = DateUtils.GetTimeStamp(now);
                var list = await _jobStorage.GetDueDealyJobId(DateUtils.GetTimeStamp(now.AddMilliseconds(delayMillSecond)));
                foreach (var item in list)
                {
                    if (context.IsShutdownRequested) break;
                    if (item.Value > maxScore)
                    {
                        diffMillSecond = item.Value - maxScore;
                        break;
                    }
                   // Console.WriteLine("延时任务到期加入即时任务队列:");
                    await _jobStorage.DueDealyJobEnqueue(item.Key);
                }

                if (list.Count == 0)
                {
                    delay = (int)delayMillSecond;

                }
                else if (diffMillSecond > 0)
                {
                    delay = (int)diffMillSecond;
                }

            },()=>Task.CompletedTask);
            // Console.WriteLine("delay:"+delay);
            if (context.IsShutdownRequested) return;
            _jobStorage.WaitForDelayJob(TimeSpan.FromMilliseconds((int)delay), context.CancellationToken);
        }
    }
}
