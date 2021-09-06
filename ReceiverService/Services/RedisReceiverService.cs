using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Mappers;
using ReceiverService.Providers;
using ReceiverService.Repositories;
using StackExchange.Redis;

namespace ReceiverService.Services
{
    public class RedisReceiverService : IHostedService
    {
        private readonly ILogger<RedisReceiverService> _logger;
        private readonly RedisProvider _redisProvider;
        private static IConfiguration _configuration;
        private readonly IRedisRepository _redisRepository;
        private readonly BlockedQueueService _blockedQueueService;
        private readonly RootToExtendedRootMapper _mapper;
        private readonly ServiceBusSenderService _serviceBusSenderService;

        public RedisReceiverService(ILogger<RedisReceiverService> logger, IConfiguration configuration, RedisProvider redisProvider, IRedisRepository redisRepository, BlockedQueueService blockedQueueService, RootToExtendedRootMapper mapper, ServiceBusSenderService serviceBusSenderService)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _redisRepository = redisRepository;
            _blockedQueueService = blockedQueueService;
            _mapper = mapper;
            _serviceBusSenderService = serviceBusSenderService;

            if (_configuration == null)
            {
                _configuration = configuration;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // IDatabase database = _redisProvider.GetDatabase();
            // Console.WriteLine(database.ListLeftPop("roots"));

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuration["CacheConnection"]);
            ISubscriber sub = redis.GetSubscriber();
            
            sub.SubscribeAsync("roots", (channel, message) => {
                _logger.LogInformation("Received message: " + message + " from channel: " + channel);
                var root = JsonSerializer.Deserialize<Root>(message);
                var extendedRoot = _mapper.Map(root, 43);

                _blockedQueueService.Add(extendedRoot);
                if (_blockedQueueService.bc.Count == 5)
                {
                    // _serviceBusSenderService.SendMessage();
                }
            });
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}