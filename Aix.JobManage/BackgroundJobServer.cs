using Aix.JobManage.Logging;
using Aix.JobManage.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.JobManage
{

    public class BackgroundJobServer
    {
        // private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private ILog _log = LogProviderFactory.GetLogger<BackgroundJobServer>();

        private BackgroundJobServerOptions _options;
        private List<IBackgroundProcess> _process;
        private IDataProcess _dataProcess;
        private volatile bool _isStart = false;

        public BackgroundJobServer(BackgroundJobServerOptions options, IDataProcess dataProcess)
        {
            _options = options;
            _dataProcess = dataProcess;
            InitProcess();
        }

        private void InitProcess()
        {
            _process = new List<IBackgroundProcess>();

            for (int i = 0; i < _options.WorkerThreadCount; i++)
            {
                _process.Add(new WorkerProcess(JobStorage.Current, _options, _dataProcess));
            }
            for (int i = 0; i < _options.DelayedJobThreadCount; i++)
            {
                _process.Add(new DelayedJobScheduler(JobStorage.Current, _options));
            }
            for (int i = 0; i < _options.RecurringJobThreadCount; i++)
            {
                _process.Add(new RecurringJobScheduler(JobStorage.Current, _options));
            }

            _process.Add(new ErrorWorkerProcess(JobStorage.Current, _options));
            _process.Add(new DeadLetterProcess(JobStorage.Current, _options));
        }


        public Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            _isStart = true;
            var context = new BackgroundProcessContext(cancellationToken, _options);
            foreach (var process in _process)
            {
                Task.Factory.StartNew(() => RunProcess(process, context), TaskCreationOptions.LongRunning);
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _isStart = false;
            return Task.CompletedTask;
        }


        private async Task RunProcess(IBackgroundProcess process, BackgroundProcessContext context)
        {
            while (_isStart && !context.IsShutdownRequested)
            {
                // Console.WriteLine("执行循环："+ process.GetType().Name);
                try
                {
                    await process.Execute(context); //内部控制异常
                }
                catch (Exception ex)
                {
                    string errorMsg = $"执行任务{process.GetType().FullName}异常：{ex.Message},{ex.StackTrace}";
                    _log.Error(errorMsg);
                }

            }
        }


    }
}
