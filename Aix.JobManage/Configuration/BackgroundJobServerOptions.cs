using Aix.JobManage.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
    public class BackgroundJobServerOptions
    {
        public static BackgroundJobServerOptions Current;

        private string[] _queues;

        public BackgroundJobServerOptions()
        {
            WorkerThreadCount = Environment.ProcessorCount * 5;
            DelayedJobThreadCount = 1;
            RecurringJobThreadCount = 1;

            Queues = new[] { JobHelper.DefaultQueue };

            ExecuteTimeout = TimeSpan.FromMinutes(10);
            FetchTimeout = TimeSpan.FromMinutes(1);
            ErrorReEnqueueInterval = TimeSpan.FromMinutes(1);
            ErrorTryCount = 5;

            RecurringJobTimeout = TimeSpan.FromMinutes(1);

            DeadLetterMaxLength = 10*10000;

            BackgroundJobServerOptions.Current = this;
        }

        /// <summary>
        /// 即时任务工作线程数 默认 Environment.ProcessorCount * 5
        /// </summary>
        public int WorkerThreadCount { get; set; }

        /// <summary>
        /// 延迟任务工作线程数 默认1
        /// </summary>
        public int DelayedJobThreadCount { get; set; }

        /// <summary>
        /// 循环任务工作线程数 默认1
        /// </summary>
        public int RecurringJobThreadCount { get; set; }

        /// <summary>
        /// 订阅队列 默认队列为JobHelper.DefaultQueue
        /// </summary>
        public string[] Queues
        {
            get { return _queues; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 0) throw new ArgumentException("You should specify at least one queue to listen.", nameof(value));

                _queues = value;
            }
        }

        /// <summary>
        /// 执行超时时间，超过该时间，任务执行错误尝试重试
        /// </summary>
        public TimeSpan ExecuteTimeout { get; set; }

        /// <summary>
        /// 即时任务抓取延时时间 默认1分钟 （即时抓取是根据发布订阅实现）
        /// </summary>
        public TimeSpan FetchTimeout { get; set; }

        /// <summary>
        /// 错误数据重新入队  线程执行间隔
        /// </summary>
        public TimeSpan ErrorReEnqueueInterval { get; set; }

        /// <summary>
        /// 最大错误次数
        /// </summary>
        public int ErrorTryCount { get; set; }

        /// <summary>
        /// 定时任务延迟时间 默认1分钟 （即时抓取是根据发布订阅实现）
        /// </summary>
        public TimeSpan RecurringJobTimeout { get; set; }

        /// <summary>
        /// 死信队列最大长度
        /// </summary>
        public int DeadLetterMaxLength { get; set; }
    }
}
