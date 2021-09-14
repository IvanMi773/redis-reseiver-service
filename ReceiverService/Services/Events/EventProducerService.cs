using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Repositories;
using ReceiverService.Services.BlockedQueue;

namespace ReceiverService.Services.Events
{
    public class EventProducerService : IEventProducerService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger<EventProducerService> _logger;

        public EventProducerService(IBlockedQueueService blockedQueueService, IRedisRepository redisRepository, 
            ILogger<EventProducerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _redisRepository = redisRepository;
            _logger = logger;
        }

        public void ProduceMessageToQueue()
        {
            Task.Run(() =>
            {
                while (true)
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
            });
        }
    }
}