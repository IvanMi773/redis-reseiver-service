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
    public class RedisReceiverService : IHostedService, IDisposable
    {
        private readonly ILogger<RedisReceiverService> _logger;
        private readonly RedisProvider _redisProvider;
        private IConfiguration _configuration;
        private readonly IRedisRepository _redisRepository;
        private readonly BlockedQueueService _blockedQueueService;
        private readonly RootToExtendedRootMapper _mapper;
        private readonly ServiceBusSenderService _serviceBusSenderService;
        private Timer _timer;
        private int TimeAfterReceivingLastEvent;

        public RedisReceiverService(ILogger<RedisReceiverService> logger, IConfiguration configuration,
            RedisProvider redisProvider, IRedisRepository redisRepository, BlockedQueueService blockedQueueService,
            RootToExtendedRootMapper mapper, ServiceBusSenderService serviceBusSenderService)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _redisRepository = redisRepository;
            _blockedQueueService = blockedQueueService;
            _mapper = mapper;
            _serviceBusSenderService = serviceBusSenderService;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ReceiveMessage, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            return Task.CompletedTask;
        }

        private async void ReceiveMessage(object state)
        {
            TimeAfterReceivingLastEvent += 500;
            
            IDatabase database = _redisProvider.GetDatabase();
            var redisEvent = database.ListLeftPop("roots");

            if (redisEvent.ToString() == null)
            {
                return;
            }
            
            var extendedRoot = _mapper.Map(JsonSerializer.Deserialize<Root>(redisEvent), 43);
            await _blockedQueueService.Add(extendedRoot);

            if (_blockedQueueService.CountOfElements == 5 || TimeAfterReceivingLastEvent >= 2000)
            {
                var messages = new string[_blockedQueueService.CountOfElements];
                for (int i = 0; i < _blockedQueueService.CountOfElements; i++)
                {
                    string msg = JsonSerializer.Serialize(_blockedQueueService.Take());
                    messages[i] = msg;
                }

                await _serviceBusSenderService.SendMessage(messages);
                _logger.LogInformation("sent messages");

                TimeAfterReceivingLastEvent = 0;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}