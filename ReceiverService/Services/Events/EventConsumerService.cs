using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Mappers;
using ReceiverService.Repositories;
using ReceiverService.Services.BlockedQueue;
using ReceiverService.Services.ServiceBus;
using StackExchange.Redis;

namespace ReceiverService.Services.Events
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger<EventConsumerService> _logger;
        private List<ExtendedRoot> _messages;

        public EventConsumerService(IBlockedQueueService blockedQueueService,
            IServiceBusSenderService serviceBusSenderService, IRedisRepository redisRepository,
            ILogger<EventConsumerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _redisRepository = redisRepository;
            _logger = logger;
            _messages = new List<ExtendedRoot>();
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


                    var extendedRoot = RootToExtendedRootMapper.Map(root, 43);

                    _messages.Add(extendedRoot);
                    if (_messages.Count == 10)
                    {
                        SendMessages();
                    }
                }
            });
        }

        private void SendMessages()
        {
            var mesgs = new List<string>();
            _messages = _messages.OrderBy(o => o.Timestamp).ToList();
            
            foreach (var message in _messages)
            {
                Console.WriteLine(JsonSerializer.Serialize(message));

                var timestamp = _redisRepository.GetFromHash("ingest-event-hub-hash", message.Id);
                DateTime.TryParse(message.Timestamp, out var eventTimestampDate);
                DateTime.TryParse(timestamp, out var redisTimestampDate);
                if (string.IsNullOrEmpty(timestamp) || DateTime.Compare(eventTimestampDate, redisTimestampDate) > 0)
                {
                    mesgs.Add(JsonSerializer.Serialize(message));
                }
                else
                {
                    _logger.LogInformation("event don't added");
                }
            }

            _serviceBusSenderService.SendMessage(mesgs);
            _messages.Clear();
        }
    }
}