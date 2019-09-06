using Aix.JobManage;
using Aix.JobManage.Extends;
using Common.BizJobHandlers;
using Common.ConsoleLog;
using Common.NLogLog;
using System;

namespace ServerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //com 虚拟机 192.168.111.130:6379
            //home 192.168.72.129:6379
            var host = "127.0.0.1:6379";
           // host = "192.168.111.131:6379";
            GlobalConfiguration.Configuration.UseRedis(host, new RedisStorageOptions { Prefix = "aixjobdemo:" })
                               //.UseLogProvider(new ConsoleLogProvider(ConsoleLogLevel.Error));
                               .UseLogProvider(new NLogProvider());

            var options = new BackgroundJobServerOptions
            {
                WorkerThreadCount = Environment.ProcessorCount * 5,
                ErrorReEnqueueInterval = TimeSpan.FromMinutes(1),
                ErrorTryCount=3,
                 Queues = new string[] { JobHelper.DefaultQueue, "delayqueue", "convertorderqueue", "auditqueue", "dispatchqueue", "inventoryqueue", "ordersendqueue" }// orderqueue  "inventoryqueue"

            };

            JobHandlerManage jobHandlerManage = new JobHandlerManage();

            jobHandlerManage.AddHandler(new OrderSendJobHandler());
            jobHandlerManage.AddHandler(new OrderConvertJobHandler());
            jobHandlerManage.AddHandler(new OrderDispatchJobHandler());


            BackgroundJobServer server = new BackgroundJobServer(options, jobHandlerManage);
            server.Start().Wait();

            Console.Read();
        }
    }
}
