using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Repositories;

namespace ReceiverService.Services
{
    public class RedisReceiverService : IHostedService, IDisposable
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IRedisRepository _redisRepository;
        private Timer _timer;
        private readonly ILogger<RedisReceiverService> _logger;
        private readonly IProcessMessagesService _processMessagesService;

        public RedisReceiverService(IBlockedQueueService blockedQueueService,
            IRedisRepository redisRepository, ILogger<RedisReceiverService> logger,
            IProcessMessagesService processMessagesService)
        {
            _blockedQueueService = blockedQueueService;
            _redisRepository = redisRepository;
            _logger = logger;
            _processMessagesService = processMessagesService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ReceiveMessage, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        private void ReceiveMessage(object state)
        {
            var isAddedEvent = false;
            while (true)
            {
                string redisEvent;
                
                try
                {
                    redisEvent = _redisRepository.PopStringFromList("roots");
                }
                catch (Exception)
                {
                    _logger.LogError("Error while pop message from redis");
                    return;
                }

                if (string.IsNullOrEmpty(redisEvent))
                {
                    if (!isAddedEvent && _blockedQueueService.CountOfElements() >= 1)
                    {
                        _processMessagesService.GetMessagesFromQueueAndSendToServiceBus();
                    }

                    break;
                }

                _blockedQueueService.Add(JsonSerializer.Deserialize<Root>(redisEvent));
                isAddedEvent = true;
                if (_blockedQueueService.CountOfElements() == 5)
                {
                    _processMessagesService.GetMessagesFromQueueAndSendToServiceBus();
                }
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