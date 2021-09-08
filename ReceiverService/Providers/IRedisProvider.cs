using StackExchange.Redis;

namespace ReceiverService.Providers
{
    public interface IRedisProvider
    {
        public IDatabase GetDatabase();
    }
}