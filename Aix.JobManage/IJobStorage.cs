using Aix.JobManage.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.JobManage
{
    public interface IJobStorage
    {
        #region client

        Task<bool> Enqueue(JobData job);

        Task<bool> EnqueueDealy(JobData job, TimeSpan timeSpan);

        Task<bool> AddRecurringJob(RecurringJobData job);

        #endregion

        #region 即时任务

        Task<FetchJobData> FetchNextJob(BackgroundProcessContext context, CancellationToken cancellationToken);

        Task SetStatusSuccess(FetchJobData fetchJobData, long performanceDuration, string executeResult);

        Task SetStatusFail(FetchJobData fetchJobData, string reason);

        #endregion

        #region 定时任务

        Task<string[]> GetAllRecurringJobId();

        Task<RecurringJobData> GetRecurringJobData(string jobId);

        Task SetRecurringJobExecuteTime(string id, long timestamp);

        Task RemoveRecurringJob(string jobId);

        void WaitForRecurringJob(TimeSpan timeSpan, CancellationToken cancellationToken);
        #endregion

        #region 延时任务

        Task<IDictionary<string, long>> GetDueDealyJobId(long timeStamp);

        Task DueDealyJobEnqueue(string jobId);

        void WaitForDelayJob(TimeSpan timeSpan, CancellationToken cancellationToken);
        #endregion

        #region 错误的数据处理

        Task<string[]> GetErrorJobId(string queue, int start, int end);

        Task RemoveErrorJobId(string queue, string jobId);

        Task RemoveErrorJobIdAndBak(string queue, string jobId);

        Task ReEnqueue(string queue, string jobId);

        /// <summary>
        /// 失败的任务，根据延迟时间进入延迟队列等待重试
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        Task<bool> ErrorJobToEnqueueDealy(string queue, string jobId, TimeSpan timeSpan);

        Task SetJobCheckedTime(string queue, string jobId, string checkedTime);


        #endregion

        #region 私信队列数据

        Task<string[]> GetDeadLetterJobId(int start, int end);


        Task RemoveDeadLetterJobId(string jobId);


        Task<bool> IsIsExistsJobData(string jobId);


        #endregion

        Task<string[]> GetAllQueues();

        Task<JobData> GetJobData(string jobId);
        Task Lock(string key, TimeSpan span, Func<Task> action, Func<Task> concurrentCallback = null);

        Task ClearJobDataIfNotExists(string jobId);
    }
}
