using Aix.JobManage;
using Common.BizJobHandlers;
using Aix.JobManage.Extends;
using Common.Utils;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace ClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //192.168.111.130    
            //home 192.168.72.129
            var host = "127.0.0.1:6379";
            host = "192.168.111.131:6379";
            GlobalConfiguration.Configuration.UseRedis(host, new RedisStorageOptions { Prefix = "aixjobdemo:" });//192.168.102.108

            int index = 0;
            #region 定时任务
            //BackgroundJobClient.Instance.AddRecurringJob("job1", "定时处理任务1", "任务1", "*/10 * * * *");

            // BackgroundJobClient.Instance.AddRecurringJob("job2", "定时处理任务2", "定时处理任务2", "*/5 * * * * *");

//            BackgroundJobClient.Instance.AddRecurringJob("job2", "定时处理任务2", new OrderConvertJobModel { OrderId = "1000" }, "*/5 * * * * *");

            //BackgroundJobClient.Instance.AddRecurringJob("job2", "定时处理任务2", new OrderConvertJobModel { OrderId = "2000" }, "*/30 * * * * *");

            #endregion

            int count = 10;

            #region 延时任务
            Task.Run(() =>
            {
                // Parallel.For(0, 1000*10000, (i) =>
                //{
                //    var delay = i % 10000;
                //    BackgroundJobClient.Instance.AddDelayJob("延迟任务" + i + "  " + DateTime.Now.AddSeconds(delay), TimeSpan.FromSeconds(delay), "delayqueue").Wait();
                //});

            });

            Task.Run(() =>
            {
                Parallel.For(0, count, async (i) =>
                {
                    var delay = i % 100;
                    var model = new OrderConvertJobModel { OrderId = Interlocked.Increment(ref index).ToString() };
                    await BackgroundJobClient.Instance.AddDelayJob(model, TimeSpan.FromSeconds(delay));
                    Console.WriteLine($"添加延时任务：{i}  " + JsonUtils.ToJson(model));

                    // await BackgroundJobClient.Instance.AddJob("转单任务" + i + " " + DateTime.Now, "convertorderqueue");
                    //await Task.Delay(50);
                });
                Console.WriteLine("添加延时任务完成");
            });
            #endregion
           
            #region 即时任务
            Task.Run(() =>
            {
                Parallel.For(0, count, async (i) =>
              {
                  var model = new OrderConvertJobModel { OrderId = Interlocked.Increment(ref index).ToString() };
                  await BackgroundJobClient.Instance.AddJob(model);
                  Console.WriteLine($"添加转单任务：{i}  " + JsonUtils.ToJson(model));

                  // await BackgroundJobClient.Instance.AddJob("转单任务" + i + " " + DateTime.Now, "convertorderqueue");
                  //await Task.Delay(50);
              });
                Console.WriteLine("添加转单任务完成");
            });
            Task.Run(() =>
            {
                Parallel.For(0, count, async (i) =>
                {
                    var model = new OrderSendJobModel { OrderId = Interlocked.Increment(ref index).ToString() };
                    await BackgroundJobClient.Instance.AddJob(model);
                    Console.WriteLine($"添加发货任务：{i}  " + JsonUtils.ToJson(model));
                    //await BackgroundJobClient.Instance.AddJob("审单任务" + i + " " + DateTime.Now, "auditqueue");
                    // await Task.Delay(50);
                });
                Console.WriteLine("添加发货任务完成");
            });



            Task.Run(() =>
            {
                Parallel.For(0, count, async (i) =>
                {
                    var model = new OrderDispatchJobModel { OrderId = Interlocked.Increment(ref index).ToString() };
                    await BackgroundJobClient.Instance.AddJob(model);
                    Console.WriteLine($"添加配货任务：{i}  " + JsonUtils.ToJson(model));
                    //await BackgroundJobClient.Instance.AddJob("审单任务" + i + " " + DateTime.Now, "auditqueue");
                    // await Task.Delay(50);
                });
                Console.WriteLine("添加配货任务完成");
            });

            //Task.Run(() =>
            //{
            //    Parallel.For(0, count, async (i) =>
            //    {
            //        await BackgroundJobClient.Instance.AddJob("库存任务" + i + " " + DateTime.Now, "inventoryqueue");
            //        await Task.Delay(50);
            //    });
            //    Console.WriteLine("添加库存任务完成");
            //});
            #endregion


            Console.WriteLine("end");
            Console.Read();

        }
    }




}
