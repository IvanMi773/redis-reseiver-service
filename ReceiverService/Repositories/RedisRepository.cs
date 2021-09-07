using Microsoft.Extensions.Logging;
using ReceiverService.Providers;
using StackExchange.Redis;

namespace ReceiverService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _database;

        public RedisRepository(RedisProvider redisProvider)
        {
            _database = redisProvider.GetDatabase();
        }

        public void PushStringToList(string listName, string str)
        {
            _database.ListRightPush(listName, str);
        }

        public string PopStringFromList(string listName)
        {
            return _database.ListLeftPop("roots");
        }
    }
}