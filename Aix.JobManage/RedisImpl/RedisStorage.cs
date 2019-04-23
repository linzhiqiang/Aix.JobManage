using Aix.JobManage.RedisImpl;
using Aix.JobManage.Server;
using Aix.JobManage.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.JobManage
{
    public class RedisStorage : IJobStorage
    {
        private IConnectionMultiplexer _redis = null;
        private IDatabase _database;
        private RedisStorageOptions _options;
        private readonly RedisSubscription _queueJobChannelSubscription;
        private readonly RedisSubscription _delayJobChannelSubscription;
        private readonly RedisSubscription _recurringJobChannelSubscription;
        public RedisStorage(IConnectionMultiplexer redis, RedisStorageOptions options)
        {
            this._redis = redis;
            this._options = options;
            _database = redis.GetDatabase(options.Db);
            _queueJobChannelSubscription = new RedisSubscription(this, redis.GetSubscriber(), "QueueJobChannel");
            _delayJobChannelSubscription = new RedisSubscription(this, redis.GetSubscriber(), "DelayJobChannel");
            _recurringJobChannelSubscription = new RedisSubscription(this, redis.GetSubscriber(), "RecurringJobChannel");
        }

        #region client
        /// <summary>
        /// 添加即时任务
        /// </summary>
        /// <param name="job"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public Task<bool> Enqueue(JobData job)
        {
            var values = job.ToDictionary().ToHashEntries();

            var trans = _database.CreateTransaction();

            trans.SetAddAsync(GetQueueSetName(), job.Queue);
            trans.HashSetAsync(GetJobHashId(job.JobId), values);
            trans.KeyExpireAsync(GetJobHashId(job.JobId), _options.JobExpireTime);

            trans.ListLeftPushAsync(GetQueueName(job.Queue), job.JobId);

            trans.PublishAsync(_queueJobChannelSubscription.Channel, job.JobId);
            var result = trans.Execute();

            return Task.FromResult(result);
        }

        /// <summary>
        /// 延时任务
        /// </summary>
        /// <param name="job"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public Task<bool> EnqueueDealy(JobData job, TimeSpan timeSpan)
        {
            var values = job.ToDictionary().ToHashEntries();
            var trans = _database.CreateTransaction();

            trans.SetAddAsync(GetQueueSetName(), job.Queue);
            trans.HashSetAsync(GetJobHashId(job.JobId), values);
            trans.KeyExpireAsync(GetJobHashId(job.JobId), _options.JobExpireTime);

            trans.SortedSetAddAsync(GetDelaySortedSetName(), job.JobId, DateUtils.GetTimeStamp(DateTime.Now.AddMilliseconds(timeSpan.TotalMilliseconds))); //当前时间戳，

            trans.PublishAsync(_delayJobChannelSubscription.Channel, job.JobId);
            var result = trans.Execute();

            return Task.FromResult(result);
        }

       

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public Task<bool> AddRecurringJob(RecurringJobData job)
        {
            var values = job.ToDictionary().ToHashEntries();
            var trans = _database.CreateTransaction();

            trans.SetAddAsync(GetQueueSetName(), job.Queue);
            trans.KeyDeleteAsync(GetRecurringJobHashId(job.JobId));
            trans.HashSetAsync(GetRecurringJobHashId(job.JobId), values);
            trans.SetAddAsync(GetRecurringJobSetName(), job.JobId);

            trans.PublishAsync(_recurringJobChannelSubscription.Channel, job.JobId);
            var result = trans.Execute();

            return Task.FromResult(result);
        }

        #endregion

        #region 即时任务

        public Task<FetchJobData> FetchNextJob(BackgroundProcessContext context, CancellationToken cancellationToken)
        {
            string jobId = null;
            string data = null;
            string queueName = null;
            var queues = context.Options.Queues;
            var randomQueues = JobHelper.RandomArray(queues);
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                //for (int i = 0; i < queues.Length; i++)
                //{
                //    queueName = queues[i];
                //    var queueKey = GetQueueName(queueName);
                //    var fetchedKey = GetProcessingQueueName(queueName);
                //    jobId = _database.ListRightPopLeftPush(queueKey, fetchedKey);
                //    if (jobId != null)
                //    {
                //        data = _database.HashGet(GetJobHashId(jobId), "Data");//取出数据字段
                //        if (data != null) break;
                //        else _database.ListRemove(GetProcessingQueueName(queueName), jobId); //可能该任务已被执行完删除
                //    }
                //}

                foreach (var item in randomQueues)
                {
                    queueName = item;
                    var queueKey = GetQueueName(queueName);
                    var fetchedKey = GetProcessingQueueName(queueName);
                    jobId = _database.ListRightPopLeftPush(queueKey, fetchedKey);
                    if (jobId != null)
                    {
                        data = _database.HashGet(GetJobHashId(jobId), "Data");//取出数据字段
                        if (data != null) break;
                        else _database.ListRemove(GetProcessingQueueName(queueName), jobId); //可能该任务已被执行完删除
                    }
                }

                if (jobId == null)
                {
                    _queueJobChannelSubscription.WaitForJob(context.Options.FetchTimeout, cancellationToken);
                    //await Task.Delay(1000);
                }
            }
            while (jobId == null);

            _database.HashSet(GetJobHashId(jobId), new HashEntry[] {
                     new HashEntry(nameof(JobData.ExecuteTime),DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                     new HashEntry(nameof(JobData.Status),1) //0 待执行，1 执行中，2 成功，9 失败
             }); //修改执行时间，状态信息


            return Task.FromResult(new FetchJobData { JobId = jobId, Queue = queueName, Data = data });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fetchJobData"></param>
        /// <param name="performanceDuration">执行时间</param>
        /// <param name="executeResult">执行结果</param>
        /// <returns></returns>
        public Task SetStatusSuccess(FetchJobData fetchJobData, long performanceDuration, string executeResult)
        {
            var trans = _database.CreateTransaction();
            trans.ListRemoveAsync(GetProcessingQueueName(fetchJobData.Queue), fetchJobData.JobId);

            //trans.HashSetAsync(GetJobHashId(fetchJobData.JobId), new HashEntry[] {
            //         new HashEntry(nameof(JobData.Status),2), //0 待执行，1 执行中，2 成功，9 失败
            //         new HashEntry("PerformanceDuration",performanceDuration)
            //     });

            //trans.ListLeftPushAsync(GetSuccessList(), fetchJobData.JobId);

            //插入history （执行日志状态的每每一步变化） 入队，执行中，执行成功，执行失败，重新入队等操作，可以查看每一步执行时间及结果

            trans.KeyDeleteAsync(GetJobHashId(fetchJobData.JobId));

            //成功的数据插入到车成功日志表中  success: jobidlist,success:state（hash：执行时间等）

            var result = trans.Execute();

            return Task.CompletedTask;
        }

        public Task SetStatusFail(FetchJobData fetchJobData, string reason)
        {
            var trans = _database.CreateTransaction();

            trans.HashIncrementAsync(GetJobHashId(fetchJobData.JobId), nameof(JobData.ErrorCount), 1);
            trans.HashSetAsync(GetJobHashId(fetchJobData.JobId), new HashEntry[] {
                     new HashEntry(nameof(JobData.Status),9) //0 待执行，1 执行中，2 成功，9 失败
                 });
            //失败数据插入到失败日志表中 error:jobidlist  error:state
            trans.Execute();

            return Task.CompletedTask;
        }
        #endregion

        #region 定时任务

        public Task<string[]> GetAllRecurringJobId()
        {
            var list = _database.SetMembers(GetRecurringJobSetName());
            return Task.FromResult(list.ToStringArray());
        }

        public Task<RecurringJobData> GetRecurringJobData(string jobId)
        {
            var dict = (_database.HashGetAll(GetRecurringJobHashId(jobId))).ToStringDictionary();

            RecurringJobData result = new RecurringJobData
            {
                JobId = dict.ContainsKey(nameof(RecurringJobData.JobId)) ? dict[nameof(RecurringJobData.JobId)] : "",
                CreateTime = dict.ContainsKey(nameof(RecurringJobData.CreateTime)) ? dict[nameof(RecurringJobData.CreateTime)] : "",
                Cron = dict.ContainsKey(nameof(RecurringJobData.Cron)) ? dict[nameof(RecurringJobData.Cron)] : "",
                Data = dict.ContainsKey(nameof(RecurringJobData.Data)) ? dict[nameof(RecurringJobData.Data)] : "",
                LastExecuteTime = dict.ContainsKey(nameof(RecurringJobData.LastExecuteTime)) ? dict[nameof(RecurringJobData.LastExecuteTime)] : "",
                Queue = dict.ContainsKey(nameof(RecurringJobData.Queue)) ? dict[nameof(RecurringJobData.Queue)] : ""
            };
            if (string.IsNullOrEmpty(result.JobId)) result = null;
            return Task.FromResult(result);
        }

        public Task SetRecurringJobExecuteTime(string jobId, long timestamp)
        {
            _database.HashSet(GetRecurringJobHashId(jobId), nameof(RecurringJobData.LastExecuteTime), timestamp);
            return Task.CompletedTask;
        }

        public Task RemoveRecurringJob(string jobId)
        {
            var trans = _database.CreateTransaction();

            trans.SetRemoveAsync(GetRecurringJobSetName(), jobId);
            trans.KeyDeleteAsync(GetRecurringJobHashId(jobId));

            trans.Execute();

            return Task.CompletedTask;
        }

        public void WaitForRecurringJob(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            _recurringJobChannelSubscription.WaitForJob(timeSpan, cancellationToken);
        }

        #endregion

        #region 延时任务

        public Task<IDictionary<string, long>> GetDueDealyJobId(long timeStamp)
        {
            var nowTimeStamp = timeStamp;// DateUtils.GetTimeStamp();
            var result = _database.SortedSetRangeByScoreWithScores(GetDelaySortedSetName(), double.NegativeInfinity, nowTimeStamp, Exclude.None, Order.Ascending, 0, 20);
            IDictionary<string, long> dict = new Dictionary<string, long>();
            foreach (SortedSetEntry item in result)
            {
                dict.Add(item.Element, (long)item.Score);
            }
            return Task.FromResult(dict);

            // var result = await _database.SortedSetRangeByScoreAsync(GetDelaySortedSetName(), double.NegativeInfinity, nowTimeStamp, Exclude.None, Order.Ascending, 0, 20);
            //return result.ToStringArray();
        }

        public Task DueDealyJobEnqueue(string jobId)
        {
            var queueName = _database.HashGet(GetJobHashId(jobId), nameof(JobData.Queue));

            var trans = _database.CreateTransaction();

            trans.ListLeftPushAsync(GetQueueName(queueName), jobId);
            trans.SortedSetRemoveAsync(GetDelaySortedSetName(), jobId);
            trans.PublishAsync(_queueJobChannelSubscription.Channel, jobId);

            trans.Execute();

            return Task.CompletedTask;
        }

        public void WaitForDelayJob(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            _delayJobChannelSubscription.WaitForJob(timeSpan, cancellationToken);
        }

        #endregion

        #region 错误的数据处理

        public Task<string[]> GetErrorJobId(string queue, int start, int end)
        {
            var list = _database.ListRange(GetProcessingQueueName(queue), start, end);
            return Task.FromResult(list.ToStringArray());
        }

        public Task RemoveErrorJobId(string queue, string jobId)
        {
            _database.ListRemove(GetProcessingQueueName(queue), jobId); // 移除缓存
            return Task.CompletedTask;
        }

        public Task RemoveErrorJobIdAndBak(string queue, string jobId)
        {
            var trans = _database.CreateTransaction();

            trans.ListRemoveAsync(GetProcessingQueueName(queue), jobId); // 移除缓存
            trans.ListLeftPushAsync(GetDeadLetterQueueName(), jobId);//加入死信队列
            trans.ListTrimAsync(GetDeadLetterQueueName(), 0, BackgroundJobServerOptions.Current.DeadLetterMaxLength);
            trans.Execute();

            return Task.CompletedTask;
        }



        public Task ReEnqueue(string queue, string jobId)
        {

            //var trans = _database.CreateTransaction();

            //trans.HashSetAsync(GetJobHashId(jobId), nameof(JobData.Status), 0);
            //trans.HashDeleteAsync(GetJobHashId(jobId), new RedisValue[] { nameof(JobData.CheckedTime), nameof(JobData.ExecuteTime) });


            //trans.ListRemoveAsync(GetProcessingQueueName(queue), jobId); // 移除缓存
            //trans.ListLeftPushAsync(GetQueueName(queue), jobId);//加入任务队列

            //trans.PublishAsync(_queueJobChannelSubscription.Channel, jobId);

            //return trans.ExecuteAsync();


            var trans = _database.CreateTransaction();

            trans.HashSetAsync(GetJobHashId(jobId), nameof(JobData.Status), 0);
            trans.HashDeleteAsync(GetJobHashId(jobId), new RedisValue[] { nameof(JobData.CheckedTime), nameof(JobData.ExecuteTime) });


            trans.ListRemoveAsync(GetProcessingQueueName(queue), jobId); // 移除缓存
            trans.ListLeftPushAsync(GetQueueName(queue), jobId);//加入任务队列

            trans.PublishAsync(_queueJobChannelSubscription.Channel, jobId);

            trans.Execute();

            return Task.CompletedTask;
        }

        /// <summary>
        /// 失败的任务，根据延迟时间进入延迟队列等待重试
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public Task<bool> ErrorJobToEnqueueDealy(string queue,string jobId, TimeSpan timeSpan)
        {
            var trans = _database.CreateTransaction();

            trans.HashSetAsync(GetJobHashId(jobId), nameof(JobData.Status), 0);
            trans.HashDeleteAsync(GetJobHashId(jobId), new RedisValue[] { nameof(JobData.CheckedTime), nameof(JobData.ExecuteTime) });
            trans.ListRemoveAsync(GetProcessingQueueName(queue), jobId); // 移除缓存

            trans.SortedSetAddAsync(GetDelaySortedSetName(), jobId, DateUtils.GetTimeStamp(DateTime.Now.AddMilliseconds(timeSpan.TotalMilliseconds))); //当前时间戳，

            trans.PublishAsync(_delayJobChannelSubscription.Channel, jobId);
            var result = trans.Execute();

            return Task.FromResult(result);
        }

        public Task SetJobCheckedTime(string queue, string jobId, string checkedTime)
        {
            _database.HashSet(GetJobHashId(jobId), nameof(JobData.CheckedTime), checkedTime);
            return Task.CompletedTask;
        }
        #endregion

        #region 私信队列数据

        public Task<string[]> GetDeadLetterJobId(int start, int end)
        {
            var list = _database.ListRange(GetDeadLetterQueueName(), start, end);
            return Task.FromResult(list.ToStringArray());
        }

        public Task RemoveDeadLetterJobId(string jobId)
        {
            _database.ListRemove(GetDeadLetterQueueName(), jobId);
            return Task.CompletedTask;
        }

        public Task<bool> IsIsExistsJobData(string jobId)
        {
            var result = _database.KeyExists(GetJobHashId(jobId));
            return Task.FromResult(result);
        }

        #endregion

        public Task<string[]> GetAllQueues()
        {
            var list = _database.SetMembers(GetQueueSetName());
            return Task.FromResult(list.ToStringArray());
        }

        public Task<JobData> GetJobData(string jobId)
        {

            var dict = (_database.HashGetAll(GetJobHashId(jobId))).ToStringDictionary();

            JobData result = new JobData
            {
                JobId = dict.ContainsKey(nameof(JobData.JobId)) ? dict[nameof(JobData.JobId)] : "",
                CreateTime = dict.ContainsKey(nameof(JobData.CreateTime)) ? dict[nameof(JobData.CreateTime)] : "",
                Data = dict.ContainsKey(nameof(JobData.Data)) ? dict[nameof(JobData.Data)] : "",
                ExecuteTime = dict.ContainsKey(nameof(JobData.ExecuteTime)) ? dict[nameof(JobData.ExecuteTime)] : "",
                Queue = dict.ContainsKey(nameof(JobData.Queue)) ? dict[nameof(JobData.Queue)] : "",
                Status = dict.ContainsKey(nameof(JobData.Status)) ? NumberUtils.ToInt(dict[nameof(JobData.Status)]) : 0,
                CheckedTime = dict.ContainsKey(nameof(JobData.CheckedTime)) ? dict[nameof(JobData.CheckedTime)] : "",
                ErrorCount = dict.ContainsKey(nameof(JobData.ErrorCount)) ? NumberUtils.ToInt(dict[nameof(JobData.ErrorCount)]) : 0,
            };
            if (string.IsNullOrEmpty(result.JobId))
            {
                result = null;
            }
            return Task.FromResult(result);
        }
        public async Task Lock(string key, TimeSpan span, Func<Task> action, Func<Task> concurrentCallback = null)
        {
            string token = Guid.NewGuid().ToString();
            if (_database.LockTake(GetRedisKey(key), token, span))
            {
                try
                {
                    await action();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _database.LockRelease(GetRedisKey(key), token);
                }
            }
            else
            {
                if (concurrentCallback != null) await concurrentCallback();
                else throw new Exception("出现并发key=" + GetRedisKey(key));
            }
        }


        public Task ClearJobDataIfNotExists(string jobId)
        {
            var value = _database.HashGet(GetJobHashId(jobId), nameof(JobData.JobId));
            if (value.HasValue == false)
            {
                _database.KeyDelete(GetJobHashId(jobId));
            }
            return Task.CompletedTask;
        }

        #region private 

        private string GetQueueName(string queue)
        {
            return GetRedisKey("queue:" + queue);
        }

        private string GetProcessingQueueName(string queue)
        {
            return GetQueueName(queue + ":processing");
        }

        private string GetJobHashId(string jobId)
        {
            return GetRedisKey("jobdata" + $":job:{jobId}");
        }

        private string GetSuccessList()
        {
            return GetRedisKey("success");
        }

        private string GetFailList()
        {
            return GetRedisKey("fail");
        }

        private string GetDeadLetterQueueName()
        {
            return GetRedisKey("queue:deadletter");
        }

        private string GetQueueSetName()
        {
            return GetRedisKey("queue:queues");
        }

        private string GetDelaySortedSetName()
        {
            return GetRedisKey("delay:jobid");
        }

        private string GetRecurringJobHashId(string jobId)
        {
            return GetRedisKey("recurring:jobdata" + $":job:{jobId}");
        }

        private string GetRecurringJobSetName()
        {
            return GetRedisKey("recurring:jobid");
        }


        #endregion

        public string GetRedisKey(string key)
        {
            return _options.Prefix + "aixjob:" + key;
        }

    }



}
