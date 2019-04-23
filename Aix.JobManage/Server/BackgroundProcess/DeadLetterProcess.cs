using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    /// <summary>
    /// 死信队列移除 jobdata不存在的数据移除
    /// </summary>
    public class DeadLetterProcess : IBackgroundProcess
    {
        private ILog _log = LogProviderFactory.GetLogger<DeadLetterProcess>();
        public IJobStorage _jobStorage;
        private BackgroundJobServerOptions _options;
        private int PerBatchSize = 100;

        public DeadLetterProcess(IJobStorage jobStorage, BackgroundJobServerOptions options)
        {
            _jobStorage = jobStorage;
            _options = options;
        }

        public async Task Execute(BackgroundProcessContext context)
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            var lockKey = "deadletter:lock";
            await _jobStorage.Lock(lockKey, TimeSpan.FromMinutes(2), async () =>
            {
                int deleteCount = 0;
                var length = 0;
                var startTime = DateTime.Now;

                var step = PerBatchSize * -1;
                var start = PerBatchSize * -1;
                var end = -1;
                do
                {
                    if (context.IsShutdownRequested) break;
                    var list = await _jobStorage.GetDeadLetterJobId(start, end);
                    length = list.Length;
                    deleteCount = await ProcessList(context,list);

                    start = start + step + deleteCount;
                    end = end + step + deleteCount;

                    //if (length > 0 && (DateTime.Now - startTime).Seconds >= 30)
                    //{
                    //    //重新设置lockKey有效期1分钟
                    //}
                }
                while (length > 0);

            }, () => Task.CompletedTask);

            if (context.IsShutdownRequested) return;
            await Task.Delay(TimeSpan.FromHours(1));
        }

        private async Task<int> ProcessList(BackgroundProcessContext context,string[] list)
        {
            int deleteCount = 0;
            for (var i = list.Length - 1; i >= 0; i--)
            {
                if (context.IsShutdownRequested) break;
                var jobId = list[i];
                if ((await _jobStorage.IsIsExistsJobData(jobId)) == false)
                {
                    await _jobStorage.RemoveDeadLetterJobId(jobId);
                    deleteCount++;
                }
            }

            return deleteCount;
        }
    }
}
