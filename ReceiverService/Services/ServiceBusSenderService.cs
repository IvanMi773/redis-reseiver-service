using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReceiverService.Services
{
    public class ServiceBusSenderService
    {
        private static IConfiguration _configuration;
        private readonly ILogger<ServiceBusSenderService> _logger;

        private static ServiceBusClient _client;
        private static ServiceBusSender _sender;

        public ServiceBusSenderService(IConfiguration configuration, ILogger<ServiceBusSenderService> logger)
        {
            _logger = logger;
            if (_configuration == null)
            {
                _configuration = configuration;
            }
        }
        
        public async Task SendMessage(string[] messages)
        {
            _client = new ServiceBusClient(_configuration["ServiceBusConnection"]);
            _sender = _client.CreateSender(_configuration["TopicName"]);

            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

            for (int i = 0; i < messages.Length; i++)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(messages[i])))
                {
                    throw new Exception("The message is to large");
                }
            }

            try
            {
                await _sender.SendMessagesAsync(messageBatch);
                _logger.LogInformation("A batch has been published");
            }
            finally
            {
                await _sender.DisposeAsync();
                await _client.DisposeAsync();
            }
        }
    }
}