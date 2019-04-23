using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
    public class RedisStorageOptions
    {
        public const string DefaultPrefix = "aix:";

        public RedisStorageOptions()
        {
            Db = 0;
            Prefix = DefaultPrefix;
            JobExpireTime = TimeSpan.FromDays(30);
            //FifoQueues = new string[0];
        }

        public string Prefix { get; set; }
        public int Db { get; set; }

        //public string[] FifoQueues { get; set; }

        /// <summary>
        /// job有效期 默认一个月
        /// </summary>
        public TimeSpan JobExpireTime { get; set; }


    }
}
