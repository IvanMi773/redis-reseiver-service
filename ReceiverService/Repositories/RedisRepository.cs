using ReceiverService.Providers;
using StackExchange.Redis;

namespace ReceiverService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _database;

        public RedisRepository(IRedisProvider redisProvider)
        {
            _database = redisProvider.GetDatabase();
        }

        public void PushStringToList(string listName, string str)
        {
            _database.ListRightPush(listName, str);
        }

        public string PopStringFromList(string listName)
        {
            return _database.ListLeftPop(listName);
        }

        public string GetFromHash(string hashName, string key)
        {
            var data = _database.HashGet(hashName, key);
            _database.HashDelete(hashName, key);
            return data;
        }
    }
}