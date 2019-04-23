using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    /// <summary>
    /// 具体执行任务接口
    /// </summary>
    public interface IDataProcess
    {
        Task<string> Process(ProcessContext context);
    }

    /// <summary>
    /// 执行任务参数
    /// </summary>
    public class ProcessContext
    {
       public string Queue { get; set; }

        public string Data { get; set; }

        public string JobId { get; set; }

        public string AckId { get; set; }
    }
}
