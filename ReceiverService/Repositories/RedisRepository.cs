using Microsoft.Extensions.Logging;
using ReceiverService.Providers;
using StackExchange.Redis;

namespace ReceiverService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisProvider _redisProvider;
        private readonly ILogger<RedisRepository> _logger;

        public RedisRepository(RedisProvider redisProvider, ILogger<RedisRepository> logger)
        {
            _redisProvider = redisProvider;
            _logger = logger;
        }

        public void PushStringToList(string listName, string str)
        {
            IDatabase cache = _redisProvider.GetDatabase();
            _logger.LogInformation(cache.ListRightPush(listName, str).ToString());
        }
    }
}