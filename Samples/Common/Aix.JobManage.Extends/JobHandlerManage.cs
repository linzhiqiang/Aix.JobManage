using Aix.JobManage.Server;
using Common.Exceptions;
using Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Extends
{
    public class JobHandlerManage:IDataProcess
    {
        Dictionary<string, IJobHandler> Handlers = new Dictionary<string, IJobHandler>();

       // public static JobHandlerManage Instance = new JobHandlerManage();

        public JobHandlerManage AddHandler<T>(JobHandlerBase<T> handler) where T: JobBaseModel
        {
            var type = JobModelAttribute.GetJobType(typeof(T));
            Handlers.Add(type, handler);
            return this;
        }

        public async Task<string> Process(ProcessContext processContext)
        {
            string result = string.Empty;
            try
            {
                var model = JsonUtils.FromJson<JobModelWraper>(processContext.Data);
                if (Handlers.ContainsKey(model.Type))
                {
                    var handler = Handlers[model.Type];
                    result = await handler.Process(model.Data);
                }
            }
            catch (BizException ex)
            {
                //业务异常不重试，不应该抛异常，返回业务异常信息即可
                result = ex.Message;
            }
            catch (Exception)
            {
                //log
                throw;
            }
            return result;
        }

        
    }
}
