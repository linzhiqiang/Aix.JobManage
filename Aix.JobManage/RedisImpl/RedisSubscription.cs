using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.JobManage.RedisImpl
{
    internal class RedisSubscription
    {
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private readonly RedisStorage _storage;
        private readonly ISubscriber _subscriber;

        public RedisSubscription(RedisStorage storage, ISubscriber subscriber, string subscriberChannel)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Channel = _storage.GetRedisKey(subscriberChannel);

            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _subscriber.Subscribe(Channel, (channel, value) =>
            {
                _mre.Set();
                //Console.WriteLine("触发："+ channel);
            });
        }

        public string Channel { get; }

        public void WaitForJob(TimeSpan timeout, CancellationToken cancellationToken)
        {
             _mre.Reset();
            WaitHandle.WaitAny(new[] { _mre, cancellationToken.WaitHandle }, timeout);
           
        }
    }
}
