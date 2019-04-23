using Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Extends
{
    internal interface IJobHandler
    {
        Task<string> Process(string jobData);
    }
    public abstract class JobHandlerBase<T> : IJobHandler where T: JobBaseModel
    {
        public async Task<string> Process(string jobData)
        {
            var obj = JsonUtils.FromJson<T>(jobData);
            var result = await Execute(obj);
            return JsonUtils.ToJson(result);
        }

        public abstract Task<object> Execute(T jobData);
    }
}
