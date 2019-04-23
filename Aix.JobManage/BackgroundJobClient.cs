using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage
{
    public class BackgroundJobClient
    {
        private IJobStorage _jobStorage;

        public static BackgroundJobClient Instance = new BackgroundJobClient();
        private BackgroundJobClient()
        {
            _jobStorage = JobStorage.Current;
        }



        #region 即时任务


        /// <summary>
        /// 添加即时任务
        /// </summary>
        /// <param name="jobData"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Task AddJob(string jobData, string queue=null)
        {
            if (string.IsNullOrEmpty(queue))
            {
                queue = JobHelper.DefaultQueue;
            }
            var job = JobData.CreateJobData(jobData, queue);
            return _jobStorage.Enqueue(job);
        }

        #endregion

        #region  延时任务

        /// <summary>
        /// 添加延时任务
        /// </summary>
        /// <param name="jobData"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public Task AddDelayJob(string jobData, TimeSpan timeSpan, string queue=null)
        {
            if (string.IsNullOrEmpty(queue))
            {
                queue = JobHelper.DefaultQueue;
            }
            var job = JobData.CreateJobData(jobData, queue);
            return _jobStorage.EnqueueDealy(job, timeSpan);
        }


        #endregion

        #region 定时任务

        public Task AddRecurringJob(string jobId, string jobName, string jobData, string cron, string queue=null)
        {
            if (string.IsNullOrEmpty(queue))
            {
                queue = JobHelper.DefaultQueue;
            }
            var job = RecurringJobData.CreateJobData(jobId, jobName, jobData, cron, queue);
            return _jobStorage.AddRecurringJob(job);
        }

        public Task RemoveRecurringJob(string jobId)
        {
            return _jobStorage.RemoveRecurringJob(jobId);
        }

        #endregion


    }
}
