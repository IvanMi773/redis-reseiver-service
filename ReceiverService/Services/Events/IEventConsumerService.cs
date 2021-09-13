using System.Threading.Tasks;

namespace ReceiverService.Services.Events
{
    public interface IEventConsumerService
    {
        public Task ConsumeMessagesFromQueue();
    }
}