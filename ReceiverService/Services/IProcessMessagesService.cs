using System.Threading.Tasks;

namespace ReceiverService.Services
{
    public interface IProcessMessagesService
    {
        public Task GetMessagesFromQueueAndSendToServiceBus();
    }
}