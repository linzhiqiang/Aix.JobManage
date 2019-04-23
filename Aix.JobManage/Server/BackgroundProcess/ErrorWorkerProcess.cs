using Aix.JobManage.Logging;
using Aix.JobManage.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    /// <summary>
    /// 执行中队列，错误数据处理 重新入队
    /// </summary>
    public class ErrorWorkerProcess : IBackgroundProcess
    {
        private ILog _log = LogProviderFactory.GetLogger<ErrorWorkerProcess>();
        private int PerBatchSize = 50;
        public IJobStorage _jobStorage;
        private BackgroundJobServerOptions _options;

        public ErrorWorkerProcess(IJobStorage jobStorage, BackgroundJobServerOptions options)
        {
            _jobStorage = jobStorage;
            _options = options;
        }
        public async Task Execute(BackgroundProcessContext context)
        {
            try
            {
                var allQueues = await _jobStorage.GetAllQueues();
                //if (allQueues.Length == 0) await Task.Delay(_options.ErrorReEnqueueInterval);
                foreach (var queue in allQueues)
                {
                    var lockKey = $"queue:queues:{queue}:lock";
                    await _jobStorage.Lock(lockKey, TimeSpan.FromMinutes(1), async () =>
                    {
                        await ProcessQueue(context,queue);
                    }, () => Task.CompletedTask);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"执行任务ErrorWorkerProcess异常：{ex.Message},{ex.StackTrace}";
                _log.Error(errorMsg);
            }
            finally
            {
                await Task.Delay(_options.ErrorReEnqueueInterval);
            }
        }

        private async Task ProcessQueue(BackgroundProcessContext context,string queue)
        {
            int deleteCount = 0;
            var length = 0;

            var step = PerBatchSize * -1;
            var start = PerBatchSize * -1;
            var end = -1;
            do
            {
                //var start = (index + 1) * PerBatchSize * -1;
                //var end = (index * PerBatchSize + 1) * -1;
                var list = await _jobStorage.GetErrorJobId(queue, start, end);
                length = list.Length;
                deleteCount = await ProcessFailedJob(context,queue, list);

                start = start + step + deleteCount;
                end = end + step + deleteCount;

            }
            while (length > 0);
        }

        public async Task<int> ProcessFailedJob(BackgroundProcessContext context,string queue, string[] list)
        {
            int deleteCount = 0;
            for (var i = list.Length - 1; i >= 0; i--)
            {
                if (context.IsShutdownRequested) break;
                var jobId = list[i];
                JobData jobData = await _jobStorage.GetJobData(jobId);
                if (jobData == null)
                {
                    await _jobStorage.RemoveErrorJobId(queue, jobId);
                    deleteCount++;
                    continue;
                }
                if (jobData.ErrorCount >= _options.ErrorTryCount)
                {
                    await _jobStorage.RemoveErrorJobIdAndBak(queue, jobId);
                    deleteCount++;
                    continue;
                }
                if (jobData.Status == 9)
                {
                    //可以加入延迟队列，这样更优
                    var delayMinute = GetWaitTime(jobData.ErrorCount);
                    //Console.WriteLine("延迟:"+ delayMinute);
                    await _jobStorage.ErrorJobToEnqueueDealy(queue, jobId, TimeSpan.FromMinutes(delayMinute));
                    deleteCount++;

                    //if (DateUtils.ToDateTime(jobData.ExecuteTime) <= DateTime.Now.AddMinutes(-1 * GetWaitTime(jobData.ErrorCount)))
                    //{
                    //    await _jobStorage.ReEnqueue(queue, jobId);//重新入队
                    //    deleteCount++;
                    //}
                }

                else if (jobData.Status == 1)
                {
                    //万一执行成功 删除失败了呢，这里不就重复执行了，这里只能业务上控制了
                    if (!string.IsNullOrEmpty(jobData.ExecuteTime) && DateUtils.ToDateTime(jobData.ExecuteTime) <= DateTime.Now.AddSeconds(-1 * _options.ExecuteTimeout.TotalSeconds))
                    {
                        await _jobStorage.ReEnqueue(queue, jobId);
                        deleteCount++;
                        _log.Error($"执行任务可能存在异常,执行时间太长或执行中断,jobId={jobId},data={jobData.Data}");
                    }
                }
                else if (jobData.Status == 0)
                {
                    if (string.IsNullOrEmpty(jobData.CheckedTime))
                    {
                        await _jobStorage.SetJobCheckedTime(queue, jobId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        await _jobStorage.ClearJobDataIfNotExists(jobId);
                    }
                    else if (DateUtils.ToDateTime(jobData.CheckedTime) <= DateTime.Now.AddMinutes(-3))
                    {
                        await _jobStorage.ReEnqueue(queue, jobId);
                        deleteCount++;
                    }
                }
            }
            return deleteCount;
        }

        private int GetWaitTime(int errorCount)
        {
            if (errorCount <= 0) errorCount = 1;
            var rand = new Random(Guid.NewGuid().GetHashCode());
            var nextTry = rand.Next(
                (int)Math.Pow(errorCount, 2), (int)Math.Pow(errorCount + 1, 2) + 1);

            return nextTry;
        }
    }
}
