using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReceiverService.Entities;
using ReceiverService.Mappers;
using ReceiverService.Repositories;

namespace ReceiverService.Services
{
    public class RedisReceiverService : IHostedService, IDisposable
    {
        private readonly BlockedQueueService _blockedQueueService;
        private readonly ServiceBusSenderService _serviceBusSenderService;
        private readonly IRedisRepository _redisRepository;
        private Timer _timer;
        private int _timeAfterSentLastButch;

        public RedisReceiverService(BlockedQueueService blockedQueueService, ServiceBusSenderService serviceBusSenderService, 
            IRedisRepository redisRepository)
        {
            _blockedQueueService = blockedQueueService;
            _serviceBusSenderService = serviceBusSenderService;
            _redisRepository = redisRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ReceiveMessage, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            return Task.CompletedTask;
        }

        private async void ReceiveMessage(object state)
        {
            var redisEvent = _redisRepository.PopStringFromList("roots");

            if (_timeAfterSentLastButch >= 2000 && _blockedQueueService.CountOfElements >= 1)
            {
                SendMessages();
            }
            
            if (string.IsNullOrEmpty(redisEvent))
            {
                _timeAfterSentLastButch += 200;
                return;
            }

            _timeAfterSentLastButch = 0;
            var extendedRoot = RootToExtendedRootMapper.Map(JsonSerializer.Deserialize<Root>(redisEvent), 43);
            await _blockedQueueService.Add(extendedRoot);

            if (_blockedQueueService.CountOfElements == 5)
            {
                SendMessages();
            }
        }

        private async void SendMessages()
        {
            _timeAfterSentLastButch = 0;
            var messages = new string[_blockedQueueService.CountOfElements];
            for (var i = 0; i < _blockedQueueService.CountOfElements; i++)
            {
                var msg = JsonSerializer.Serialize(_blockedQueueService.Take());
                messages[i] = msg;
            }

            _blockedQueueService.Clear();
            await _serviceBusSenderService.SendMessage(messages);
            _timeAfterSentLastButch = 0;
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