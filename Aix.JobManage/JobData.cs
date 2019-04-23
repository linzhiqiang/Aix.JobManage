using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
    public class JobData
    {
        public string JobId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 执行时间 
        /// </summary>
        public string ExecuteTime { get; set; }

        /// <summary>
        /// 0 待执行，1 执行中，2 成功，9 失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 业务数据
        /// </summary>
        public string Data { get; set; }

        public int ErrorCount { get; set; }


        public string CheckedTime { get; set; }

        public string Queue { get; set; }

        public IDictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string> {
                { "JobId",JobId},
                { "CreateTime",CreateTime ?? string.Empty},
                { "ExecuteTime",ExecuteTime ??string.Empty},
                { "Status",Status.ToString()},
                { "Data",Data ?? string.Empty},
                { "ErrorCount",ErrorCount.ToString()},
                { "CheckedTime",CheckedTime ??string.Empty},
                { "Queue",Queue}
            };
        }


        public static JobData CreateJobData(string data, string queue)
        {
            return new JobData
            {
                JobId = Guid.NewGuid().ToString(),
                Data = data,
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = 0,
                ErrorCount = 0,
                Queue = queue
            };
        }
    }

    public class RecurringJobData
    {
        public string JobId { get; set; }

        public string JobName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 执行时间 
        /// </summary>
        public string LastExecuteTime { get; set; }


        /// <summary>
        /// 业务数据
        /// </summary>
        public string Data { get; set; }

        public string Cron { get; set; }

        public string Queue { get; set; }

        public IDictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string> {
                { nameof(RecurringJobData.JobId),JobId},
                {  nameof(RecurringJobData.JobName),JobName},
                {  nameof(RecurringJobData.CreateTime),CreateTime},
                {  nameof(RecurringJobData.Data),Data},
                {  nameof(RecurringJobData.Cron),Cron},
                {  nameof(RecurringJobData.Queue),Queue}
            };
        }


        public static RecurringJobData CreateJobData(string jobId, string jobName, string data, string cron, string queue)
        {
            return new RecurringJobData
            {
                JobId = jobId,
                JobName = jobName,
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Data = data,
                Cron = cron,
                Queue = queue
            };
        }
    }

    public class FetchJobData
    {
        public string JobId { get; set; }

        public string Queue { get; set; }

        public string Data { get; set; }
    }
}
