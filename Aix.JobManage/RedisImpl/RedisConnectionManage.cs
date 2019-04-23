using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.JobManage
{
    public class RedisConnectionManage
    {
        public static RedisConnectionManage Instance = new RedisConnectionManage();
        private RedisConnectionManage() { }

        public void SetConnetionString(string connetionString)
        {
            this._Redis = ConnectionMultiplexer.Connect(connetionString);
        }

        public void SetConnectionMultiplexer(IConnectionMultiplexer connectionMultiplexer)
        {
            this._Redis = connectionMultiplexer;
        }


        private IConnectionMultiplexer _Redis = null;
       
        public IConnectionMultiplexer Connection
        {
            get
            {
                return _Redis;
            }
        }


    }
}
