using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Mappers;

namespace ReceiverService.Services
{
    public class BlockedQueueConsumerService : IHostedService
    {
        private readonly IBlockedQueueService _blockedQueueService;
        private readonly IServiceBusSenderService _serviceBusSenderService;
        private readonly ILogger<BlockedQueueConsumerService> _logger;

        public BlockedQueueConsumerService(IBlockedQueueService blockedQueueService,
            IServiceBusSenderService serviceBusSenderService, ILogger<BlockedQueueConsumerService> logger)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _logger = logger;
        }

        private void ConsumeMessagesFromQueue()
        {
            var messages = new List<string>();
            while (true)
            {
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
                        continue;
                    }
                    
                    var msg = JsonSerializer.Serialize(extendedRoot);
                    messages.Add(msg);

                    if (messages.Count == 5)
                    {
                        _serviceBusSenderService.SendMessage(messages);
                    }
                }
                Thread.Sleep(2000);
                
                if (messages.Count >= 1)
                {
                    _serviceBusSenderService.SendMessage(messages);
                }
                messages = new List<string>();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(ConsumeMessagesFromQueue, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}