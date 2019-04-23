using Aix.JobManage;
using Aix.JobManage.Extends;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage
{
  public static   class BackgroundJobClientExtensions
    {
        public static Task AddJob(this BackgroundJobClient client, JobBaseModel jobModel)
        {

            var jsonData = new JobModelWraper {
                Type = JobModelAttribute.GetJobType(jobModel.GetType()),
                Data = JsonUtils.ToJson(jobModel)
            };
            var queue = JobModelAttribute.GetJobQueue(jobModel.GetType());
           return  client.AddJob(JsonUtils.ToJson(jsonData), queue);
        }


        public static Task AddDelayJob(this BackgroundJobClient client, JobBaseModel jobModel, TimeSpan timeSpan)
        {
            var jsonData = new JobModelWraper
            {
                Type = JobModelAttribute.GetJobType(jobModel.GetType()),
                Data = JsonUtils.ToJson(jobModel)
            };
            var queue = JobModelAttribute.GetJobQueue(jobModel.GetType());
            return client.AddDelayJob(JsonUtils.ToJson(jsonData), timeSpan, queue);
        }


        public static Task AddRecurringJob(this BackgroundJobClient client, string jobId, string jobName, JobBaseModel jobModel, string cron)
        {
            var jsonData = new JobModelWraper
            {
                Type = JobModelAttribute.GetJobType(jobModel.GetType()),
                Data = JsonUtils.ToJson(jobModel)
            };
            var queue = JobModelAttribute.GetJobQueue(jobModel.GetType());
            return client.AddRecurringJob(jobId, jobName, JsonUtils.ToJson(jsonData), cron, queue);
        }


    }
}
