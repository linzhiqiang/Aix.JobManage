using Aix.JobManage.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Aix.JobManage.Server
{
    public class WorkerProcess : IBackgroundProcess
    {
        private ILog _log = LogProviderFactory.GetLogger<WorkerProcess>();
        public IJobStorage _jobStorage;
        IDataProcess _dataProcess;
        private BackgroundJobServerOptions _options;

        public WorkerProcess(IJobStorage jobStorage, BackgroundJobServerOptions options, IDataProcess dataProcess)
        {
            _jobStorage = jobStorage;
            _options = options;
            _dataProcess = dataProcess;
        }
        public async Task Execute(BackgroundProcessContext context)
        {
            var fetchData = await _jobStorage.FetchNextJob(context, context.CancellationToken);
            _log.Trace($"开始执行任务：{fetchData.JobId}");

            try
            {
                var duration = Stopwatch.StartNew();
                var result = await DoWork(fetchData);//执行任务
                duration.Stop();
                _log.Trace($"执行任务成功：{fetchData.JobId}");
                var performanceDuration = duration.ElapsedMilliseconds;//执行任务的毫秒数，可以记录到hash中

                await _jobStorage.SetStatusSuccess(fetchData, performanceDuration, result);
            }
            catch (Exception ex)//失败修改状态为失败
            {
                string errorMsg = $"执行任务出错，Queue={fetchData.Queue},Data={fetchData.Data},Message={ex.Message},StackTrace={ex.StackTrace}";
                await _jobStorage.SetStatusFail(fetchData, errorMsg);
                _log.Error(errorMsg);
            }

        }

        private Task<string> DoWork(FetchJobData fetchJobData)
        {
            ProcessContext context = new ProcessContext
            {
                Data = fetchJobData.Data,
                Queue = fetchJobData.Queue,
                JobId = fetchJobData.JobId,
                AckId= fetchJobData.JobId
            };
            return _dataProcess.Process(context);
        }
    }
}
