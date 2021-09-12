using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ReceiverService.Mappers;
using ReceiverService.Services.BlockedQueue;
using ReceiverService.Services.ServiceBus;

namespace ReceiverService.Services.Events
{
    public class EventConsumerService : IEventConsumerService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private List<string> _messages;

        public EventConsumerService(IBlockedQueueService blockedQueueService, IServiceBusSenderService serviceBusSenderService)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _messages = new List<string>(); 
        }

        public void ConsumeMessagesFromQueue()
        {
            Task.Run(() =>
            {
                while (true)
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
                    
                    var serializedExtendedRoot = JsonSerializer.Serialize(RootToExtendedRootMapper.Map(root, 43));
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
            _messages = new List<string>();
        }
    }
}