using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReceiverService.Mappers;
using ReceiverService.Repositories;
using ReceiverService.Services.BlockedQueue;
using ReceiverService.Services.ServiceBus;

namespace ReceiverService.Services.Events
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger<EventConsumerService> _logger;
        private readonly List<string> _messages;

        public EventConsumerService(IBlockedQueueService blockedQueueService,
            IServiceBusSenderService serviceBusSenderService, IRedisRepository redisRepository, ILogger<EventConsumerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _redisRepository = redisRepository;
            _logger = logger;
            _messages = new List<string>();
        }

        public Task ConsumeMessagesFromQueue()
        {
            return Task.Factory.StartNew(() =>
            {
                while (!_blockedQueueService.IsCompleted())
                {
                    var root = _blockedQueueService.Take(2000);

                    if (root == null)
                    {
                        if (_messages.Count >= 1)
                        {
                            SendMessages();
                        }

                        continue;
                    }
                    
                    var timestamp = _redisRepository.GetFromHash("ingest-event-hub-hash", root.Id);
                    
                    if (string.IsNullOrEmpty(timestamp))
                    {
                        continue;
                    } 
                    
                    var rt = RootToExtendedRootMapper.Map(root, 43);
                    var serializedExtendedRoot = JsonSerializer.Serialize(rt);
                    _messages.Add(serializedExtendedRoot);
                    if (_messages.Count == 5)
                    {
                        SendMessages();
                    }
                }
            });
        }

        private void SendMessages()
        {
            _serviceBusSenderService.SendMessage(_messages);
            _messages.Clear();
        }
    }
}