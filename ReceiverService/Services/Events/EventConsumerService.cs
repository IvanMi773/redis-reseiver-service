using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ReceiverService.Entities;
using ReceiverService.Mappers;
using ReceiverService.Services.BlockedQueue;
using ReceiverService.Services.ServiceBus;

namespace ReceiverService.Services.Events
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private readonly List<string> _messages;

        public EventConsumerService(IBlockedQueueService blockedQueueService,
            IServiceBusSenderService serviceBusSenderService)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
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