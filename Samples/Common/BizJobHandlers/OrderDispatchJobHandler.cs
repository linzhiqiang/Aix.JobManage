using Aix.JobManage.Extends;
using System;
using System.Threading.Tasks;

namespace Common.BizJobHandlers
{
    [JobModel(Type = "OrderDispatchJobModel", Queue = "dispatchqueue")]
    public class OrderDispatchJobModel : JobBaseModel
    {

        public string OrderId { get; set; }
    }

    public class OrderDispatchJobHandler : JobHandlerBase<OrderDispatchJobModel>
    {
        public override Task<object> Execute(OrderDispatchJobModel jobData)
        {
            Console.WriteLine($"配货服务:{jobData.OrderId} " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            // throw new Exception("发货异常");
            return Task.FromResult(new object());
        }
    }
   
}
