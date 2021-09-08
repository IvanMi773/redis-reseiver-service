using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReceiverService.Services
{
    public class ServiceBusSenderService : IServiceBusSenderService, IDisposable
    {
        private readonly ILogger<ServiceBusSenderService> _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusSenderService(IConfiguration configuration, ILogger<ServiceBusSenderService> logger)
        {
            _logger = logger;
            
            _client = new ServiceBusClient(configuration["ServiceBusConnection"]);
            _sender = _client.CreateSender(configuration["TopicName"]);
        }
        
        public async Task SendMessage(string[] messages)
        {
            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

            foreach (var message in messages)
            {
                try
                {
                    messageBatch.TryAddMessage(new ServiceBusMessage(message));
                }
                catch (Exception)
                {
                    _logger.LogError("Cannot add message to batch");
                }
            }

            try
            {
                await _sender.SendMessagesAsync(messageBatch);
                _logger.LogInformation("A batch has been published");
            }
            catch (Exception)
            {
                _logger.LogError("Error while sending batch");
            }
        }

        public async void Dispose()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}