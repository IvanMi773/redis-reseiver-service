using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReceiverService.Services
{
    public class ServiceBusSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusSenderService> _logger;

        private ServiceBusClient _client;
        private ServiceBusSender _sender;

        public ServiceBusSenderService(IConfiguration configuration, ILogger<ServiceBusSenderService> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task SendMessage(string[] messages)
        {
            _client = new ServiceBusClient(_configuration["ServiceBusConnection"]);
            _sender = _client.CreateSender(_configuration["TopicName"]);

            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

            foreach (var t in messages)
            {
                try
                {
                    messageBatch.TryAddMessage(new ServiceBusMessage(t));
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
            finally
            {
                await _sender.DisposeAsync();
                await _client.DisposeAsync();
            }
        }
    }
}