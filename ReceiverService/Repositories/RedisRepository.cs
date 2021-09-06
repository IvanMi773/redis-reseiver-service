using Microsoft.Extensions.Logging;
using ReceiverService.Providers;
using StackExchange.Redis;

namespace ReceiverService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisProvider _redisConfig;
        private readonly ILogger<RedisRepository> _logger;

        public RedisRepository(RedisProvider redisConfig, ILogger<RedisRepository> logger)
        {
            _redisConfig = redisConfig;
            _logger = logger;
        }

        public void PushStringToList(string str)
        {
            IDatabase cache = _redisConfig.GetDatabase();
            _logger.LogInformation(cache.ListRightPush("roots", str).ToString());
        }
    }

    public interface IRedisRepository
    {
        public void PushStringToList(string str);
    }
}