using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReceiverService.Services.ServiceBus
{
    public interface IServiceBusSenderService
    {
        public Task SendMessage(List<string> messages);
    }
}