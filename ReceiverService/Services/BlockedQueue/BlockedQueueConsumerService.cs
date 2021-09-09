using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Mappers;
using ReceiverService.Services.ServiceBus;

namespace ReceiverService.Services.BlockedQueue
{
    public class BlockedQueueConsumerService : IHostedService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private readonly ILogger<BlockedQueueConsumerService> _logger;
        private Timer _timer;

        public BlockedQueueConsumerService(IBlockedQueueService blockedQueueService,
            IServiceBusSenderService serviceBusSenderService, ILogger<BlockedQueueConsumerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _logger = logger;
        }

        private void ConsumeMessagesFromQueue(object state)
        {
            var messages = new List<string>();
            while (_blockedQueueService.CountOfElements() > 0)
            {
                ExtendedRoot extendedRoot;
                try
                {
                    extendedRoot = RootToExtendedRootMapper.Map(_blockedQueueService.Take(), 43);
                }
                catch (InvalidOperationException)
                {
                    _logger.LogError("Error with getting message from queue");
                    break;
                }
                
                var msg = JsonSerializer.Serialize(extendedRoot);
                messages.Add(msg);
                if (messages.Count == 5)
                {
                    _serviceBusSenderService.SendMessage(messages);
                    return;
                }
            }
            
            if (messages.Count >= 1)
            {
                _serviceBusSenderService.SendMessage(messages);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ConsumeMessagesFromQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}