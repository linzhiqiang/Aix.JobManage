using Aix.JobManage.Logging;
using StackExchange.Redis;
using System;

namespace Aix.JobManage
{
    public interface IGlobalConfiguration
    {
    }

    public class GlobalConfiguration : IGlobalConfiguration
    {
        public static GlobalConfiguration Configuration { get; } = new GlobalConfiguration();

        internal GlobalConfiguration()
        {
        }
    }


    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration UseRedis(
          this IGlobalConfiguration configuration, string connectionString, RedisStorageOptions options)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            RedisConnectionManage.Instance.SetConnetionString(connectionString);

            InitRedisStorage(options);
            return configuration;
        }

        public static IGlobalConfiguration UseRedis(
          this IGlobalConfiguration configuration, IConnectionMultiplexer connectionMultiplexer, RedisStorageOptions options)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            RedisConnectionManage.Instance.SetConnectionMultiplexer(connectionMultiplexer);
            InitRedisStorage(options);

            return configuration;
        }

        private static void InitRedisStorage(RedisStorageOptions options)
        {
            RedisStorage redisStorage = new RedisStorage(RedisConnectionManage.Instance.Connection, options);
            JobStorage.Current = redisStorage;
        }

        public static void UseLogProvider(this IGlobalConfiguration configuration, ILogProvider logProvider)
        {
            LogProviderFactory.SetCurrentLogProvider(logProvider);
        }


    }
}
