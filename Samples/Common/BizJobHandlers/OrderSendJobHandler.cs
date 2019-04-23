using Aix.JobManage.Extends;
using System;
using System.Threading.Tasks;

namespace Common.BizJobHandlers
{
    [JobModel(Type = "OrderSendJobModel", Queue = "ordersendqueue")]
    public class OrderSendJobModel : JobBaseModel
    {

        public string OrderId { get; set; }
    }

    public class OrderSendJobHandler : JobHandlerBase<OrderSendJobModel>
    {
        public override Task<object> Execute(OrderSendJobModel jobData)
        {
            Console.WriteLine($"发货服务:{jobData.OrderId} " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            // throw new Exception("发货异常");
            return Task.FromResult(new object());
        }
    }
}
