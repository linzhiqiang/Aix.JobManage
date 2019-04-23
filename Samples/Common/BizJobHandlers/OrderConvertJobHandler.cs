using Aix.JobManage.Extends;
using System;
using System.Threading.Tasks;

namespace Common.BizJobHandlers
{
    [JobModel(Type = "OrderConvertJobModel", Queue= "convertorderqueue")]
    public class OrderConvertJobModel : JobBaseModel
    {

        public string OrderId { get; set; }
    }

    public class OrderConvertJobHandler : JobHandlerBase<OrderConvertJobModel>
    {
        public override Task<object> Execute(OrderConvertJobModel jobData)
        {
            Console.WriteLine($"转单服务:{jobData.OrderId} "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            return Task.FromResult(new object());
        }
    }
}
