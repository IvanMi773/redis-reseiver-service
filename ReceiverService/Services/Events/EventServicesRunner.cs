using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ReceiverService.Services.Events;

namespace ReceiverService.Services
{
    public class EventServicesRunner : IHostedService
    {
        private readonly IEventConsumerService _eventConsumerService;
        private readonly IEventProducerService _eventProducerService;
        
        public EventServicesRunner(IEventConsumerService eventConsumerService, IEventProducerService eventProducerService)
        {
            _eventConsumerService = eventConsumerService;
            _eventProducerService = eventProducerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventProducerService.ProduceMessageToQueue();
            _eventConsumerService.ConsumeMessagesFromQueue();
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}