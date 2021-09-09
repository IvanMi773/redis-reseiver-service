using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Repositories;
using ReceiverService.Services.BlockedQueue;

namespace ReceiverService.Services
{
    public class BlockedQueueProducerService : IHostedService, IDisposable
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IRedisRepository _redisRepository;
        private Timer _timer;
        private readonly ILogger<BlockedQueueProducerService> _logger;

        public BlockedQueueProducerService(IBlockedQueueService blockedQueueService,
            IRedisRepository redisRepository, ILogger<BlockedQueueProducerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _redisRepository = redisRepository;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ProduceMessageToQueue, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(200));

            return Task.CompletedTask;
        }

        private void ProduceMessageToQueue(object state)
        {
            try
            {
                var redisEvent = _redisRepository.PopStringFromList("roots");
                
                if (!string.IsNullOrEmpty(redisEvent))
                {
                    _blockedQueueService.Add(JsonSerializer.Deserialize<Root>(redisEvent));
                }
            }
            catch (Exception)
            {
                _logger.LogError("Error while pop message from redis");
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